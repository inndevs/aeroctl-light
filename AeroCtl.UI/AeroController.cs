using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

		private bool autoFan;
		public bool AutoFan
		{
			get => this.autoFan;
			set
			{
				this.autoFan = value;
				this.OnPropertyChanged();
				if(!this.updating)
				{
					this.aero.Fans.AutoFan = value;
				}
			}
		}

		private int fanAdjust;
		public int FanAdjust
		{
			get => this.fanAdjust;
			set
			{
				this.fanAdjust = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.FanAdjust = value;
				}
			}
		}

		private bool maxFan;
		public bool MaxFan
		{
			get => this.maxFan;
			set
			{
				this.maxFan = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.MaxFan = value;
				}
			}
		}

		private bool fixedFan;
		public bool FixedFan
		{
			get => this.fixedFan;
			set
			{
				this.fixedFan = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.FixedFan = value;
				}
			}
		}

		private int fixedFanSpeed;
		public int FixedFanSpeed
		{
			get => this.fixedFanSpeed;
			set
			{
				this.fixedFanSpeed = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.FixedFanSpeed = value;
				}
			}
		}

		private bool stepFan;
		public bool StepFan
		{
			get => this.stepFan;
			set
			{
				this.stepFan = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.StepFan = value;
				}
			}
		}

		private bool nvThermalTarget;
		public bool NvThermalTarget
		{
			get => this.nvThermalTarget;
			set
			{
				this.nvThermalTarget = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.NvThermalTarget = value;
				}
			}
		}

		private bool nvPowerConfig;
		public bool NvPowerConfig
		{
			get => this.nvPowerConfig;
			set
			{
				this.nvPowerConfig = value;
				this.OnPropertyChanged();
				if (!this.updating)
				{
					this.aero.Fans.NvPowerConfig = value;
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

		public AeroController(Aero aero)
		{
			this.aero = aero;
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

				this.CpuTemperature = this.aero.CpuTemperature;
				this.FanRpm1 = this.aero.Fans.Rpm1;
				this.FanRpm2 = this.aero.Fans.Rpm2;
				this.AutoFan = this.aero.Fans.AutoFan;
				this.FanAdjust = this.aero.Fans.FanAdjust;
				this.MaxFan = this.aero.Fans.MaxFan;
				this.FixedFan = this.aero.Fans.FixedFan;
				this.FixedFanSpeed = this.aero.Fans.FixedFanSpeed;
				this.StepFan = this.aero.Fans.StepFan;
				this.NvThermalTarget = this.aero.Fans.NvThermalTarget;
				this.NvPowerConfig = this.aero.Fans.NvPowerConfig;
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
