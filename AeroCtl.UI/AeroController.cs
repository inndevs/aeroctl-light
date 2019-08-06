using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using AeroCtl.UI.Properties;

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
			set
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

		private int screenBrightness;
		public int ScreenBrightness
		{
			get => (int)this.screenBrightness;
			set
			{
				this.screenBrightness = value;
				this.OnPropertyChanged();

				if (!this.updating)
				{
					this.aero.Screen.Brightness = value;
				}
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
				{
					this.aero.WifiEnabled = value;
				}
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

				if (!this.updating)
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
				{
					this.aero.Battery.ChargeStop = value;
				}
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
				{
					this.aero.Battery.ChargePolicy = value ? ChargePolicy.CustomStop : ChargePolicy.Full;
				}
			}
		}

		private bool fanProfileInvalid;

		private FanProfile fanProfile;
		public FanProfile FanProfile
		{
			get => this.fanProfile;
			set
			{
				this.fanProfile = value;
				this.OnPropertyChanged();

				if (!this.updating)
				{
					Settings.Default.FanProfile = (int)value;
					Settings.Default.Save();
					this.fanProfileInvalid = true;
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

				if (!this.updating)
				{
					Settings.Default.FixedFanSpeed = value;
					Settings.Default.Save();
					this.fanProfileInvalid = true;
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

				if (!this.updating)
				{
					Settings.Default.AutoFanAdjust = value;
					Settings.Default.Save();
					this.fanProfileInvalid = true;
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

				if (!this.updating)
				{
					Settings.Default.SoftwareFanCurve = string.Join(" ", value.Select(p => $"{p.Temperature.ToString(CultureInfo.InvariantCulture)} {p.FanSpeed.ToString(CultureInfo.InvariantCulture)}"));
					Settings.Default.Save();
					this.fanProfileInvalid = true;
				}
			}
		}

		public AeroController(Aero aero)
		{
			this.aero = aero;
		}

		public void Load()
		{
			this.updating = true;
			try
			{
				Settings s = Settings.Default;
				this.StartMinimized = s.StartMinimized;
				this.FanProfile = (FanProfile)s.FanProfile;
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
				this.fanProfileInvalid = true;
				this.updating = false;
			}
		}

		private async Task applyFanProfileAsync()
		{
			if (this.swFanController != null)
			{
				await this.swFanController.StopAsync();
				this.swFanController = null;
			}

			switch (this.FanProfile)
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
					throw new InvalidEnumArgumentException(nameof(this.FanProfile), (int) this.FanProfile, typeof(FanProfile));
			}
		}

		public async Task UpdateAsync(bool full = false)
		{
			this.updating = true;
			try
			{
				if (full)
				{
					this.BaseBoard = this.aero.BaseBoard;
					this.SerialNumber = this.aero.SerialNumber;
					this.BiosVersion = string.Join("; ", this.aero.BiosVersions);

					if (this.aero.Keyboard.Rgb != null)
					{
						this.KeyboardFWVersion = await this.aero.Keyboard.Rgb.GetFirmwareVersionAsync();
					}
				}

				if (this.fanProfileInvalid)
				{
					await this.applyFanProfileAsync();
					this.fanProfileInvalid = false;
				}

				this.CpuTemperature = await this.aero.GetCpuTemperatureAsync();
				this.GpuTemperature = await this.aero.GetGpuTemperatureAsync();
				(this.FanRpm1, this.FanRpm2) = await this.aero.Fans.GetRpmAsync();
				this.ScreenBrightness = (int)this.aero.Screen.Brightness;
				this.ChargeStopEnabled = this.aero.Battery.ChargePolicy == ChargePolicy.CustomStop;
				this.ChargeStop = this.aero.Battery.ChargeStop;
				this.WifiEnabled = this.aero.WifiEnabled;
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
				double gpu = await this.aero.GetGpuTemperatureAsync();

				return Math.Max(cpu, gpu);
			}

			public async Task SetSpeedAsync(double speed, CancellationToken cancellationToken)
			{
				await this.aero.Fans.SetFixedAsync(speed);
			}
		}
	}
}
