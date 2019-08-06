using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	/// <summary>
	/// Current state of the laptop for data binding.
	/// </summary>
	public class AeroController : INotifyPropertyChanged
	{
		private readonly Aero aero;
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

		private int cpuTemperature;
		public int CpuTemperature
		{
			get => this.cpuTemperature;
			private set
			{
				this.cpuTemperature = value;
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
				this.fanProfileInvalid = true;
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
				this.fanProfileInvalid = true;
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
				this.fanProfileInvalid = true;
			}
		}

		public AeroController(Aero aero)
		{
			this.aero = aero;
		}

		private void applyFanProfile()
		{
			switch (this.FanProfile)
			{
				case FanProfile.Quiet:
					this.aero.Fans.SetQuiet();
					break;
				case FanProfile.Normal:
					this.aero.Fans.SetNormal();
					break;
				case FanProfile.Gaming:
					this.aero.Fans.SetGaming();
					break;
				case FanProfile.Fixed:
					this.aero.Fans.SetFixed(this.fixedFanSpeed);
					break;
				case FanProfile.Auto:
					this.aero.Fans.SetAuto(this.autoFanAdjust);
					break;
				case FanProfile.Custom:
					this.aero.Fans.SetCustom();
					break;
				case FanProfile.Software:
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
					this.applyFanProfile();
					this.fanProfileInvalid = false;
				}

				this.CpuTemperature = this.aero.CpuTemperature;
				this.FanRpm1 = this.aero.Fans.Rpm1;
				this.FanRpm2 = this.aero.Fans.Rpm2;
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
	}
}
