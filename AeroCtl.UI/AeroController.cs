using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using AeroCtl.UI.Properties;
using NvAPIWrapper.DRS.SettingValues;

namespace AeroCtl.UI
{
	/// <summary>
	/// Current state of the laptop for data binding.
	/// </summary>
	public class AeroController : INotifyPropertyChanged
	{
		private readonly Aero aero;
		private SoftwareFanController swFanController;
		private bool updating;
		private bool loading;

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

		private int screenBrightness;
		public int ScreenBrightness
		{
			get => (int)this.screenBrightness;
			set
			{
				this.screenBrightness = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.Screen.Brightness = value;
			}
		}

		private bool wifiEnabled;
		public bool WifiEnabled
		{
			get => this.wifiEnabled;
			set
			{
				this.wifiEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.WifiEnabled = value;
			}
		}

		private bool touchpadEnabled;
		public bool TouchpadEnabled
		{
			get => this.touchpadEnabled;
			set
			{
				this.touchpadEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.Touchpad.Enabled = value;
			}
		}


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


		private int chargeStop;
		public int ChargeStop
		{
			get => this.chargeStop;
			set
			{
				this.chargeStop = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.Battery.ChargeStop = value;
			}
		}

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

		private int batteryHealth;
		public int BatteryHealth
		{
			get => this.batteryHealth;
			private set
			{
				this.batteryHealth = value;
				this.OnPropertyChanged();
			}
		}

		private bool smartCharge;
		public bool SmartCharge
		{
			get => this.smartCharge;
			set
			{
				this.smartCharge = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.Battery.SmartCharge = value;
			}
		}

		private bool chargeStopEnabled;
		public bool ChargeStopEnabled
		{
			get => this.chargeStopEnabled;
			set
			{
				this.chargeStopEnabled = value;
				this.OnPropertyChanged();

				if (!this.updating)
					this.aero.Battery.ChargePolicy = value ? ChargePolicy.CustomStop : ChargePolicy.Full;
			}
		}

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

		private FanPoint[] softwareFanCurve;
		public FanPoint[] SoftwareFanCurve
		{
			get => this.softwareFanCurve;
			set
			{
				this.softwareFanCurve = value;
				this.OnPropertyChanged();

				this.FanProfileInvalid = true;

				if (!this.loading)
				{
					Settings.Default.SoftwareFanCurve = string.Join(" ", value.Select(p => $"{p.Temperature.ToString(CultureInfo.InvariantCulture)} {p.FanSpeed.ToString(CultureInfo.InvariantCulture)}"));
					Settings.Default.Save();
				}
			}
		}

		private bool gpuBoost;
		public bool GpuBoost
		{
			get => this.gpuBoost;
			set
			{
				this.gpuBoost = value;
				this.OnPropertyChanged();

				if (!this.updating)
					((Aero2019GpuController)this.aero.Gpu).BoostEnabled = value;
			}
		}

		private bool gpuBoostAvailable;
		public bool GpuBoostAvailable
		{
			get => this.gpuBoostAvailable;
			private set
			{
				this.gpuBoostAvailable = value;
				this.OnPropertyChanged();
			}
		}

		private bool gpuPowerConfig;
		public bool GpuPowerConfig
		{
			get => this.gpuPowerConfig;
			set
			{
				this.gpuPowerConfig = value;
				this.OnPropertyChanged();

				if (!this.updating)
					((Aero2019GpuController)this.aero.Gpu).PowerConfig = value;
			}
		}

		private bool gpuPowerConfigAvailable;
		public bool GpuPowerConfigAvailable
		{
			get => this.gpuPowerConfigAvailable;
			private set
			{
				this.gpuPowerConfigAvailable = value;
				this.OnPropertyChanged();
			}
		}


		public AeroController(Aero aero)
		{
			this.aero = aero;
		}

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
				this.SoftwareFanCurve = s.SoftwareFanCurve
					.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
					.Select((str, i) => (i, str))
					.GroupBy(x => x.i / 2, x => x.str)
					.Select(g => (double.Parse(g.First(), CultureInfo.InvariantCulture), double.Parse(g.Last(), CultureInfo.InvariantCulture)))
					.Select(t => new FanPoint(t.Item1, t.Item2))
					.ToArray();
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
					if ((this.SoftwareFanCurve?.Length ?? 0) < 1)
					{
						await this.aero.Fans.SetNormalAsync();
					}
					else
					{
						this.swFanController = new SoftwareFanController(this.SoftwareFanCurve, FanConfig.Default, new AeroFanProvider(this.aero));
					}

					break;
				default:
					throw new InvalidEnumArgumentException(nameof(this.FanProfile), (int)newProfile, typeof(FanProfile));
			}
		}

		public async Task UpdateAsync(bool full = false)
		{
			Debug.Assert(!this.updating);

			this.updating = true;
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

				this.CpuTemperature = await this.aero.GetCpuTemperatureAsync();
				this.GpuTemperature = await this.aero.Gpu.GetTemperatureAsync();

				if (this.aero.Gpu is Aero2019GpuController newGpu)
				{
					this.GpuBoostAvailable = this.GpuPowerConfigAvailable = true;
					this.GpuBoost = newGpu.BoostEnabled;
					this.GpuPowerConfig = newGpu.PowerConfig;
				}

				this.SmartCharge = this.aero.Battery.SmartCharge;
				this.BatteryCycles = await this.aero.Battery.GetCyclesAsync();
				this.BatteryHealth = await this.aero.Battery.GetHealthAsync();
				this.BatteryCharge = await this.aero.Battery.GetRemainingCharge();
				(this.FanRpm1, this.FanRpm2) = await this.aero.Fans.GetRpmAsync();
				this.FanPwm = (await this.aero.Fans.GetPwmAsync()) * 100;
				this.ScreenBrightness = (int)this.aero.Screen.Brightness;
				this.ChargeStopEnabled = this.aero.Battery.ChargePolicy == ChargePolicy.CustomStop;
				this.ChargeStop = this.aero.Battery.ChargeStop;
				this.WifiEnabled = this.aero.WifiEnabled;
				this.TouchpadEnabled = this.aero.Touchpad.Enabled;
			}
			finally
			{
				this.updating = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private sealed class AeroFanProvider : ISoftwareFanProvider
		{
			private readonly Aero aero;

			public AeroFanProvider(Aero aero)
			{
				this.aero = aero;
			}

			public async Task<double> GetTemperatureAsync(CancellationToken cancellationToken)
			{
				double cpu = await this.aero.GetCpuTemperatureAsync();
				double gpu = await this.aero.Gpu.GetTemperatureAsync();

				return Math.Max(cpu, gpu);
			}

			public async Task SetSpeedAsync(double speed, CancellationToken cancellationToken)
			{
				await this.aero.Fans.SetFixedAsync(speed);
			}
		}
	}
}
