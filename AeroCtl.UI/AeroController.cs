using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AeroCtl.UI.Properties;
using AeroCtl.UI.SoftwareFan;

namespace AeroCtl.UI
{
	/// <summary>
	/// Current state of the laptop for data binding.
	/// </summary>
	public class AeroController : INotifyPropertyChanged
	{
		#region Fields

		private readonly Aero aero;
		private readonly HwMonitor hwMonitor;
		private SoftwareFanController swFanController;
		private readonly AsyncLocal<bool> updating;
		private bool loading;
		private readonly ConcurrentQueue<Func<Task>> updates;

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
					this.aero.Display.Brightness = value;
			}
		}

		#endregion

		#region WifiEnabled

		private bool wifiEnabled;
		public bool WifiEnabled
		{
			get => this.wifiEnabled;
			set
			{
				this.wifiEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.aero.WifiEnabled = value;
			}
		}

		#endregion

		#region TouchpadEnabled

		private bool touchpadEnabled;
		public bool TouchpadEnabled
		{
			get => this.touchpadEnabled;
			set
			{
				this.touchpadEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => this.aero.Touchpad.SetEnabledAsync(value));
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
			}
		}

		#endregion

		#region BatteryCharge

		private int batteryCharge;
		public int BatteryCharge
		{
			get => this.batteryCharge;
			private set
			{
				this.batteryCharge = value;
				this.OnPropertyChanged();
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
					this.updates.Enqueue(() => this.aero.Battery.SetSmargeChargeAsync(value));
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
					this.updates.Enqueue(() => this.aero.Battery.SetChargePolicyAsync(value ? ChargePolicy.CustomStop : ChargePolicy.Full));
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
					this.updates.Enqueue(() => this.aero.Battery.SetChargeStopAsync(value));
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
					Settings.Default.SoftwareFanConfig = new StringCollection() {value.ToJson().ToString()};
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

		#region GpuBoost

		private bool gpuBoost;
		public bool GpuBoost
		{
			get => this.gpuBoost;
			set
			{
				this.gpuBoost = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P75GpuController)this.aero.Gpu).SetBoostEnabledAsync(value));
			}
		}

		#endregion

		#region GpuPowerConfig

		private bool gpuPowerConfig;
		public bool GpuPowerConfig
		{
			get => this.gpuPowerConfig;
			set
			{
				this.gpuPowerConfig = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P75GpuController)this.aero.Gpu).SetPowerConfigAsync(value));
			}
		}

		#endregion

		#region GpuThermalTarget

		private bool gpuThermalTarget;
		public bool GpuThermalTarget
		{
			get => this.gpuThermalTarget;
			set
			{
				this.gpuThermalTarget = value;
				this.OnPropertyChanged();

				if (!this.updating.Value)
					this.updates.Enqueue(() => ((P75GpuController)this.aero.Gpu).SetThermalTargetEnabledAsync(value));
			}
		}

		#endregion

		#region Constructors

		public AeroController(Aero aero)
		{
			this.aero = aero;
			this.updating = new AsyncLocal<bool>();
			this.updates = new ConcurrentQueue<Func<Task>>();
			this.hwMonitor = new HwMonitor();
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
					await this.aero.Fans.SetQuietAsync();
					break;
				case FanProfile.Normal:
					await this.aero.Fans.SetNormalAsync();
					break;
				case FanProfile.Gaming:
					await this.aero.Fans.SetGamingAsync();
					break;
				case FanProfile.Fixed:
					await this.aero.Fans.SetFixedAsync(this.fixedFanSpeed);
					break;
				case FanProfile.Auto:
					await this.aero.Fans.SetAutoAsync(this.autoFanAdjust);
					break;
				case FanProfile.Custom:
					await this.aero.Fans.SetCustomAsync();
					break;
				case FanProfile.Software:
					this.swFanController = new SoftwareFanController(this.SoftwareFanConfig, new AeroFanProvider(this.aero, this.hwMonitor));
					break;
				default:
					throw new InvalidEnumArgumentException(nameof(this.FanProfile), (int)newProfile, typeof(FanProfile));
			}
		}

		public async Task UpdateAsync(bool full = false)
		{
			while (this.updates.TryDequeue(out var f))
				await f();

			Debug.Assert(!this.updating.Value);

			this.updating.Value = true;
			try
			{
				if (full)
				{
					this.BaseBoard = this.aero.BaseBoard;
					this.Sku = this.aero.Sku;
					this.SerialNumber = this.aero.SerialNumber;
					this.BiosVersion = string.Join("; ", this.aero.BiosVersions);

					if (this.aero.Keyboard.Rgb != null)
					{
						this.KeyboardFWVersion = await this.aero.Keyboard.Rgb.GetFirmwareVersionAsync();
					}
				}

				if (this.FanProfileInvalid)
				{
					this.FanProfileInvalid = false;
					await this.applyFanProfileAsync();
				}

				lock (this.hwMonitor)
				{
					this.hwMonitor.Update();
					this.CpuTemperature = this.hwMonitor.CpuTemperature;
					this.GpuTemperature = this.hwMonitor.GpuTemperature;
				}

				if (this.aero.Gpu is P75GpuController newGpu)
				{
					this.GpuConfigAvailable = true;
					this.GpuBoost = await newGpu.GetBoostEnabledAsync();
					this.GpuPowerConfig = await newGpu.GetPowerConfigAsync();
					this.GpuThermalTarget = await newGpu.GetThermalTargetEnabledAsync();
				}

				(this.FanRpm1, this.FanRpm2) = await this.aero.Fans.GetRpmAsync();
				this.FanPwm = await this.aero.Fans.GetPwmAsync() * 100;
				this.DisplayBrightness = (int)this.aero.Display.Brightness;

				this.SmartCharge = await this.aero.Battery.GetSmartChargeAsync();
				this.ChargeStopEnabled = await this.aero.Battery.GetChargePolicyAsync() == ChargePolicy.CustomStop;
				this.ChargeStop = await this.aero.Battery.GetChargeStopAsync();
				this.BatteryCycles = await this.aero.Battery.GetCyclesAsync();
				this.BatteryHealth = await this.aero.Battery.GetHealthAsync();
				this.BatteryCharge = await this.aero.Battery.GetRemainingChargeAsync();
				
				this.WifiEnabled = this.aero.WifiEnabled;
				this.TouchpadEnabled = await this.aero.Touchpad.GetEnabledAsync();
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

			this.hwMonitor?.Dispose();
		}

		#endregion

		#region Nested Types

		/// <summary>
		/// Provider implementation for the software fan.
		/// </summary>
		private sealed class AeroFanProvider : ISoftwareFanProvider
		{
			private readonly Aero aero;
			private readonly HwMonitor hwmon;

			public AeroFanProvider(Aero aero, HwMonitor hwmon)
			{
				this.aero = aero;
				this.hwmon = hwmon;
			}

			public ValueTask<double> GetTemperatureAsync(CancellationToken cancellationToken)
			{
				lock (this.hwmon)
				{
					this.hwmon.Update();
					double cpu = this.hwmon.CpuTemperature;
					double gpu = this.hwmon.GpuTemperature;
					return new ValueTask<double>(Math.Max(cpu, gpu));
				}
			}

			public async ValueTask SetSpeedAsync(double speed, CancellationToken cancellationToken)
			{
				if (this.aero.Fans is IDirectFanSpeedController direct)
				{
					direct.SetFixed(speed);
				}
				else
				{
					await this.aero.Fans.SetFixedAsync(speed);
				}
			}
		}

		#endregion
	}
}
