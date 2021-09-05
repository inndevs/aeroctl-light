﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AeroCtl.UI.Properties;
using AeroCtl.UI.SoftwareFan;

namespace AeroCtl.UI
{
	/// <summary>
	/// Contain the current state of the laptop for data binding and controls its various properties.
	/// </summary>
	public class AeroController : INotifyPropertyChanged
	{
		#region Fields

		private SoftwareFanController swFanController;
		private readonly AsyncLocal<bool> updating;
		private bool loading;
		private readonly ConcurrentQueue<Func<Task>> updates;

		#endregion

		#region Aero

		/// <summary>
		/// The wrapped <see cref="Aero"/> instance.
		/// </summary>
		public Aero Aero { get; }

		#endregion

		#region StartMinimized

		private bool startMinimized;
		public bool StartMinimized
		{
			get => this.startMinimized;
			set
			{
				this.startMinimized = value;
				this.OnPropertyChanged();

				if (!this.loading)
				{
					Settings.Default.StartMinimized = value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region BaseBoard

		private string baseBoard;
		public string BaseBoard
		{
			get => this.baseBoard;
			private set
			{
				this.baseBoard = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region Sku

		private string sku;
		public string Sku
		{
			get => this.sku;
			private set
			{
				this.sku = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region BiosVersion

		private string biosVersion;
		public string BiosVersion
		{
			get => this.biosVersion;
			private set
			{
				this.biosVersion = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region SerialNumber

		private string serialNumber;
		public string SerialNumber
		{
			get => this.serialNumber;
			private set
			{
				this.serialNumber = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region KeyboardFWVersion

		private Version keyboardFWVersion;
		public Version KeyboardFWVersion
		{
			get => this.keyboardFWVersion;
			private set
			{
				this.keyboardFWVersion = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region CpuTemperature

		private double cpuTemperature;
		public double CpuTemperature
		{
			get => this.cpuTemperature;
			private set
			{
				this.cpuTemperature = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region GpuTemperature

		private double gpuTemperature;
		public double GpuTemperature
		{
			get => this.gpuTemperature;
			private set
			{
				this.gpuTemperature = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region FanRpm1

		private int fanRpm1;
		public int FanRpm1
		{
			get => this.fanRpm1;
			private set
			{
				this.fanRpm1 = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region FanRpm2

		private int fanRpm2;
		public int FanRpm2
		{
			get => this.fanRpm2;
			private set
			{
				this.fanRpm2 = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region FanPwm

		private double fanPwm;
		public double FanPwm
		{
			get => this.fanPwm;
			private set
			{
				this.fanPwm = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region DisplayBrightness

		private int displayBrightness;
		public int DisplayBrightness
		{
			get => (int)this.displayBrightness;
			set
			{
				this.displayBrightness = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.Aero.Display.Brightness = value;
			}
		}

		#endregion

		#region WifiEnabled

		private bool? wifiEnabled;
		public bool? WifiEnabled
		{
			get => this.wifiEnabled;
			set
			{
				this.wifiEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value && value.HasValue)
					this.updates.Enqueue(async () => await this.Aero.SetWifiEnabledAsync(value.Value));
			}
		}

		#endregion

		#region CameraEnabled

		private bool? cameraEnabled;
		public bool? CameraEnabled
		{
			get => this.cameraEnabled;
			set
			{
				this.cameraEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value && value.HasValue)
					this.updates.Enqueue(async () => await this.Aero.SetCameraEnabledAsync(value.Value));
			}
		}

		#endregion

		#region BluetoothEnabled

		private bool bluetoothEnabled;

		public bool BluetoothEnabled
		{
			get => this.bluetoothEnabled;
			set
			{
				this.bluetoothEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => this.Aero.Bluetooth.SetEnabledAsync(value));
			}
		}
		#endregion

		#region PowerLineStatus

		private BatteryState batteryState;

		public BatteryState BatteryState
		{
			get => this.batteryState;
			private set
			{
				this.batteryState = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region BatteryString

		public string BatteryString
		{
			get
			{
				StringBuilder str = new StringBuilder();

				str.Append("Charge: ");
				str.Append(this.BatteryChargePercent);
				str.Append(" % (");
				str.Append(this.BatteryCharge.ToString("F1", CultureInfo.InvariantCulture));
				str.Append(" Wh");

				if (Math.Abs(this.BatteryChargeRate) > 0.0)
				{
					str.Append(" +");
					str.Append(this.batteryChargeRate.ToString("F1", CultureInfo.InvariantCulture));
					str.Append(" W");
				}

				if (Math.Abs(this.BatteryDischargeRate) > 0.0)
				{
					str.Append(" -");
					str.Append(this.BatteryDischargeRate.ToString("F1", CultureInfo.InvariantCulture));
					str.Append(" W");
				}

				if (Math.Abs(this.BatteryVoltage) > 0.0)
				{
					str.Append(" @ ");
					str.Append(this.BatteryVoltage.ToString("F2", CultureInfo.InvariantCulture));
					str.Append(" V");
				}

				str.Append(")");

				return str.ToString();
			}
		}

		#endregion

		#region BatteryCycles

		private int batteryCycles;
		public int BatteryCycles
		{
			get => this.batteryCycles;
			private set
			{
				this.batteryCycles = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryChargePercent

		private int batteryChargePercent;
		public int BatteryChargePercent
		{
			get => this.batteryChargePercent;
			private set
			{
				this.batteryChargePercent = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryCharge

		private double batteryCharge;
		public double BatteryCharge
		{
			get => this.batteryCharge;
			private set
			{
				this.batteryCharge = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryChargeRate

		private double batteryChargeRate;
		public double BatteryChargeRate
		{
			get => this.batteryChargeRate;
			private set
			{
				this.batteryChargeRate = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryDischargeRate

		private double batteryDischargeRate;
		public double BatteryDischargeRate
		{
			get => this.batteryDischargeRate;
			private set
			{
				this.batteryDischargeRate = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryVoltage

		private double batteryVoltage;
		public double BatteryVoltage
		{
			get => this.batteryVoltage;
			private set
			{
				this.batteryVoltage = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryHealth

		private int? batteryHealth;
		public int? BatteryHealth
		{
			get => this.batteryHealth;
			private set
			{
				this.batteryHealth = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region SmartCharge

		private bool smartCharge;
		public bool SmartCharge
		{
			get => this.smartCharge;
			set
			{
				this.smartCharge = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => this.Aero.Battery.SetSmargeChargeAsync(value));
			}
		}

		#endregion

		#region ChargeStopEnabled

		private bool chargeStopEnabled;
		public bool ChargeStopEnabled
		{
			get => this.chargeStopEnabled;
			set
			{
				this.chargeStopEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => this.Aero.Battery.SetChargePolicyAsync(value ? ChargePolicy.CustomStop : ChargePolicy.Full));

				if (!this.loading && !this.updating.Value)
				{
					Settings.Default.ChargeStop = this.ChargeStopEnabled ? this.ChargeStop : -1;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region ChargeStop

		private int chargeStop;
		public int ChargeStop
		{
			get => this.chargeStop;
			set
			{
				this.chargeStop = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => this.Aero.Battery.SetChargeStopAsync(value));

				if (!this.loading && !this.updating.Value)
				{
					Settings.Default.ChargeStop = this.ChargeStopEnabled ? this.ChargeStop : -1;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region FanProfileInvalid

		private bool fanProfileInvalid;
		public bool FanProfileInvalid
		{
			get => this.fanProfileInvalid;
			set
			{
				this.fanProfileInvalid = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region FanProfile

		private FanProfile fanProfile;
		public FanProfile FanProfile
		{
			get => this.fanProfile;
			set
			{
				this.fanProfile = value;
				this.OnPropertyChanged();

				this.FanProfileInvalid = true;

				if (!this.loading)
				{
					Settings.Default.FanProfile = (int)value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region FanProfileAlt

		private FanProfile fanProfileAlt;
		public FanProfile FanProfileAlt
		{
			get => this.fanProfileAlt;
			set
			{
				this.fanProfileAlt = value;
				this.OnPropertyChanged();

				if (!this.loading)
				{
					Settings.Default.FanProfileAlt = (int)value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region FixedFanSpeed

		private double fixedFanSpeed = 0.25;
		public double FixedFanSpeed
		{
			get => this.fixedFanSpeed;
			set
			{
				this.fixedFanSpeed = value;
				this.OnPropertyChanged();

				this.FanProfileInvalid = true;

				if (!this.loading)
				{
					Settings.Default.FixedFanSpeed = value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region AutoFanAdjust

		private double autoFanAdjust = 0.25;
		public double AutoFanAdjust
		{
			get => this.autoFanAdjust;
			set
			{
				this.autoFanAdjust = value;
				this.OnPropertyChanged();

				this.FanProfileInvalid = true;

				if (!this.loading)
				{
					Settings.Default.AutoFanAdjust = value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region SoftwareFanConfig

		private FanConfig softwareFanConfig;
		public FanConfig SoftwareFanConfig
		{
			get => this.softwareFanConfig;
			set
			{
				this.softwareFanConfig = value;
				this.OnPropertyChanged();

				this.FanProfileInvalid = true;

				if (!this.loading)
				{
					Settings.Default.SoftwareFanConfig = new StringCollection() { value.ToJson().ToString() };
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region GpuConfigAvailable

		private bool gpuConfigAvailable;
		public bool GpuConfigAvailable
		{
			get => this.gpuConfigAvailable;
			private set
			{
				this.gpuConfigAvailable = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region GpuAiBoost

		private bool gpuAiBoost;
		private bool gpuAiBoostSupported;
		public bool GpuAiBoost
		{
			get => this.gpuAiBoost;
			set
			{
				this.gpuAiBoost = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P7GpuController)this.Aero.Gpu).SetAiBoostEnabledAsync(value));
			}
		}

		public bool GpuAiBoostSupported
		{
			get => this.gpuAiBoostSupported;
			private set
			{
				this.gpuAiBoostSupported = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region GpuAiBoost

		private bool gpuDynamicBoost;
		private bool gpuDynamicBoostSupported;
		public bool GpuDynamicBoost
		{
			get => this.gpuDynamicBoost;
			set
			{
				this.gpuDynamicBoost = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P7GpuController)this.Aero.Gpu).SetDynamicBoostAsync(value));
			}
		}

		public bool GpuDynamicBoostSupported
		{
			get => this.gpuDynamicBoostSupported;
			private set
			{
				this.gpuDynamicBoostSupported = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region GpuPowerConfig

		private bool gpuPowerConfig;
		private bool gpuPowerConfigSupported;
		public bool GpuPowerConfig
		{
			get => this.gpuPowerConfig;
			set
			{
				this.gpuPowerConfig = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P7GpuController)this.Aero.Gpu).SetPowerConfigAsync(value));
			}
		}

		public bool GpuPowerConfigSupported
		{
			get => this.gpuPowerConfigSupported;
			private set
			{
				this.gpuPowerConfigSupported = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region GpuThermalTarget

		private bool gpuThermalTarget;
		private bool gpuThermalTargetSupported;
		public bool GpuThermalTarget
		{
			get => this.gpuThermalTarget;
			set
			{
				this.gpuThermalTarget = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P7GpuController)this.Aero.Gpu).SetThermalTargetEnabledAsync(value));
			}
		}

		public bool GpuThermalTargetSupported
		{
			get => this.gpuThermalTargetSupported;
			private set
			{
				this.gpuThermalTargetSupported = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region DisplayAvailable

		private bool displayAvailable;

		public bool DisplayAvailable
		{
			get => this.displayAvailable;
			private set
			{
				this.displayAvailable = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region DisplayFrequency

		private uint? displayFrequency;

		public uint? DisplayFrequency
		{
			get => this.displayFrequency;
			private set
			{
				this.displayFrequency = value;
				this.OnPropertyChanged();
			}
		}

		#endregion

		#region DisplayFrequencies

		private IReadOnlyList<uint> displayFrequencies;

		public IReadOnlyList<uint> DisplayFrequencies
		{
			get => this.displayFrequencies;
			private set
			{
				this.displayFrequencies = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.DisplayFrequencyChoices));
			}
		}

		public IReadOnlyList<uint> DisplayFrequencyChoices
		{
			get
			{
				List<uint> frequencies = new List<uint>((this.DisplayFrequencies?.Count ?? 0) + 1);

				frequencies.Add(0);

				if (this.DisplayFrequencies != null)
				{
					foreach (uint freq in this.DisplayFrequencies)
					{
						frequencies.Add(freq);
					}
				}

				return frequencies;
			}
		}

		#endregion

		#region DisplayFrequencyAc

		private uint displayFrequencyAc;

		public uint DisplayFrequencyAc
		{
			get => this.displayFrequencyAc;
			set
			{
				this.displayFrequencyAc = value;
				this.OnPropertyChanged();

				if (!this.loading)
				{
					Settings.Default.DisplayFrequencyAc = value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region DisplayFrequencyDc

		private uint displayFrequencyDc;

		public uint DisplayFrequencyDc
		{
			get => this.displayFrequencyDc;
			set
			{
				this.displayFrequencyDc = value;
				this.OnPropertyChanged();

				if (!this.loading)
				{
					Settings.Default.DisplayFrequencyDc = value;
					Settings.Default.Save();
				}
			}
		}

		#endregion

		#region Constructors

		public AeroController(Aero aero)
		{
			this.Aero = aero;
			this.updating = new AsyncLocal<bool>();
			this.updates = new ConcurrentQueue<Func<Task>>();
		}

		#endregion

		#region Methods

		public void Load()
		{
			this.loading = true;
			try
			{
				Settings s = Settings.Default;
				this.StartMinimized = s.StartMinimized;
				this.FanProfile = (FanProfile)s.FanProfile;
				this.FanProfileAlt = (FanProfile)s.FanProfileAlt;
				this.FixedFanSpeed = s.FixedFanSpeed;
				this.AutoFanAdjust = s.AutoFanAdjust;
				this.DisplayFrequencyAc = s.DisplayFrequencyAc;
				this.DisplayFrequencyDc = s.DisplayFrequencyDc;
				this.ChargeStopEnabled = s.ChargeStop >= 0;
				this.ChargeStop = s.ChargeStop >= 0 ? s.ChargeStop : 97;

				this.SoftwareFanConfig = new FanConfig();
				if (s.SoftwareFanConfig != null && s.SoftwareFanConfig.Count > 0)
					this.SoftwareFanConfig = FanConfig.FromJson((JsonObject)JsonValue.Parse(s.SoftwareFanConfig[0]));
			}
			finally
			{
				this.FanProfileInvalid = true;
				this.loading = false;
			}
		}

		private async Task applyFanProfileAsync()
		{
			FanProfile newProfile = this.FanProfile;
			Debug.WriteLine($"Applying fan profile {newProfile}");

			if (this.swFanController != null)
			{
				SoftwareFanController swCtl = this.swFanController;
				this.swFanController = null;

				await swCtl.StopAsync();
			}

			switch (newProfile)
			{
				case FanProfile.Quiet:
					await this.Aero.Fans.SetQuietAsync();
					break;
				case FanProfile.Normal:
					await this.Aero.Fans.SetNormalAsync();
					break;
				case FanProfile.Gaming:
					await this.Aero.Fans.SetGamingAsync();
					break;
				case FanProfile.Fixed:
					await this.Aero.Fans.SetFixedAsync(this.FixedFanSpeed);
					break;
				case FanProfile.Auto:
					await this.Aero.Fans.SetAutoAsync(this.AutoFanAdjust);
					break;
				case FanProfile.Custom:
					await this.Aero.Fans.SetCustomAsync();
					break;
				case FanProfile.Software:
					this.swFanController = new SoftwareFanController(this.SoftwareFanConfig, new FanProviderImpl(this));
					break;
				default:
					throw new InvalidEnumArgumentException(nameof(this.FanProfile), (int)newProfile, typeof(FanProfile));
			}
		}

		public async Task UpdateAsync(UpdateMode mode)
		{
			while (this.updates.TryDequeue(out var updateFunc))
				await updateFunc();

			Debug.Assert(!this.updating.Value);

			this.updating.Value = true;
			try
			{
				if (mode >= UpdateMode.Full)
				{
					this.BaseBoard = this.Aero.BaseBoard;
					this.Sku = this.Aero.Sku;
					this.SerialNumber = this.Aero.SerialNumber;
					this.BiosVersion = string.Join("; ", this.Aero.BiosVersions);
					this.BatteryState = this.Aero.Battery.State;

					if (this.Aero.Keyboard.Rgb != null)
						this.KeyboardFWVersion = await this.Aero.Keyboard.Rgb.GetFirmwareVersionAsync();
				}

				if (mode >= UpdateMode.Normal)
				{
					if (this.FanProfileInvalid)
					{
						this.FanProfileInvalid = false;
						await this.applyFanProfileAsync();
					}

					if (this.Aero.Gpu is P7GpuController newGpu)
					{
						this.GpuConfigAvailable = true;

						this.GpuAiBoostSupported = newGpu.AiBoostSupported;
						this.GpuPowerConfigSupported = newGpu.PowerConfigSupported;
						this.GpuDynamicBoostSupported = newGpu.DynamicBoostSupported;
						this.GpuThermalTargetSupported = newGpu.ThermalTargetSupported;

						this.GpuAiBoost = this.GpuAiBoostSupported ? await newGpu.GetAiBoostEnabledAsync() : false;
						this.GpuPowerConfig = this.GpuPowerConfigSupported ? await newGpu.GetPowerConfigAsync() : false;
						this.GpuDynamicBoost = this.GpuDynamicBoostSupported ? await newGpu.GetDynamicBoostAsync() : false;
						this.GpuThermalTarget = this.GpuThermalTargetSupported ? await newGpu.GetThermalTargetEnabledAsync() : false;
					}

					(this.FanRpm1, this.FanRpm2) = await this.Aero.Fans.GetRpmAsync();
					this.FanPwm = await this.Aero.Fans.GetPwmAsync() * 100;
					this.DisplayBrightness = this.Aero.Display.Brightness;
					this.DisplayFrequency = this.Aero.Display.GetIntegratedDisplayFrequency();
					this.DisplayAvailable = this.DisplayFrequency != null;
					this.DisplayFrequencies = this.Aero.Display.GetIntegratedDisplayFrequencies().OrderBy(hz => hz).ToImmutableArray();

					this.SmartCharge = await this.Aero.Battery.GetSmartChargeAsync();
					this.ChargeStopEnabled = await this.Aero.Battery.GetChargePolicyAsync() == ChargePolicy.CustomStop;
					this.ChargeStop = await this.Aero.Battery.GetChargeStopAsync();
					this.BatteryCycles = await this.Aero.Battery.GetCyclesAsync();
					this.BatteryHealth = await this.Aero.Battery.GetHealthAsync();

					BatteryStatus status = await this.Aero.Battery.GetStatusAsync();
					this.BatteryCharge = status.Charge;
					this.BatteryChargePercent = status.ChargePercent;
					this.BatteryChargeRate = status.ChargeRate;
					this.BatteryDischargeRate = status.DischargeRate;
					this.BatteryVoltage = status.Voltage;

					this.WifiEnabled = await this.Aero.GetWifiEnabledAsync();
					this.BluetoothEnabled = await this.Aero.Bluetooth.GetEnabledAsync();
					this.CameraEnabled = await this.Aero.GetCameraEnabledAsync();
				}

				this.CpuTemperature = await this.Aero.Cpu.GetTemperatureAsync();
				this.GpuTemperature = await this.Aero.Gpu.GetTemperatureAsync() ?? 0.0;

				BatteryState prevBatteryState = this.BatteryState;
				this.BatteryState = this.Aero.Battery.State;

				if (this.BatteryState != prevBatteryState)
				{
					if (this.BatteryState == BatteryState.DC && this.DisplayFrequencyDc > 0)
					{
						Debug.WriteLine($"Changing display frequency to {this.DisplayFrequencyDc}");
						this.Aero.Display.SetIntegratedDisplayFrequency(this.DisplayFrequencyDc);
					}

					if (this.BatteryState != BatteryState.DC && this.DisplayFrequencyAc > 0)
					{
						Debug.WriteLine($"Changing display frequency to {this.DisplayFrequencyAc}");
						this.Aero.Display.SetIntegratedDisplayFrequency(this.DisplayFrequencyAc);
					}
				}
			}
			finally
			{
				this.updating.Value = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public async ValueTask DisposeAsync()
		{
			if (this.swFanController != null)
			{
				await this.swFanController.StopAsync();
				this.swFanController = null;
			}
		}

		public async ValueTask<bool> ResetKeyboard()
		{
			await this.Aero.Keyboard.Rgb.ResetAsync();
			return true;
		}

		#endregion

		#region Nested Types

		/// <summary>
		/// <see cref="ISoftwareFanProvider"/> implementation for the software fan.
		/// </summary>
		private sealed class FanProviderImpl : ISoftwareFanProvider
		{
			private readonly AeroController controller;

			public FanProviderImpl(AeroController controller)
			{
				this.controller = controller;
			}

			public ValueTask<double> GetTemperatureAsync(CancellationToken cancellationToken)
			{
				return new(Math.Max(this.controller.CpuTemperature, this.controller.GpuTemperature));
			}

			public async ValueTask SetSpeedAsync(double speed, CancellationToken cancellationToken)
			{
				if (this.controller.Aero.Fans is IFanControllerSync syncController)
				{
					syncController.SetFixed(speed);
				}
				else
				{
					await this.controller.Aero.Fans.SetFixedAsync(speed);
				}
			}
		}

		#endregion
	}
}
