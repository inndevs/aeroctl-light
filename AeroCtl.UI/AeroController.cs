using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

		private int BatteryChargePercent
		{
			get => this.batteryChargePercent;
			set
			{
				this.batteryChargePercent = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(this.BatteryString));
			}
		}

		#endregion

		#region BatteryCharge

		private double batteryCharge;

		private double BatteryCharge
		{
			get => this.batteryCharge;
			set
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
			try {
				Settings s = Settings.Default;
				this.StartMinimized = s.StartMinimized;
				this.ChargeStopEnabled = s.ChargeStop >= 0;
				this.ChargeStop = s.ChargeStop >= 0 ? s.ChargeStop : 97;
			} finally {
				this.loading = false;
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
				}

				// Only update if UI is visible 
				if (mode >= UpdateMode.Normal)
				{ 
					(this.FanRpm1, this.FanRpm2) = await this.Aero.Fans.GetRpmAsync();
					this.FanPwm = await this.Aero.Fans.GetPwmAsync() * 100;

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
					
					this.CpuTemperature = await this.Aero.Cpu.GetTemperatureAsync();
					this.GpuTemperature = await this.Aero.Gpu.GetTemperatureAsync() ?? 0.0;
				}
			}
			finally
			{
				this.updating.Value = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
}
