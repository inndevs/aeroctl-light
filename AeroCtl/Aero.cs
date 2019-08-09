using NativeWifi;
using System;
using System.Collections.Immutable;
using System.Linq;
using NvAPIWrapper;
using NvAPIWrapper.GPU;

namespace AeroCtl
{
	/// <summary>
	/// Implements the AERO interfaces.
	/// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private ICpuController cpu;
		private IGpuController gpu;
		private IFanController fans;
		private KeyboardController keyboard;
		private BatteryController battery;
		private DisplayController display;
		private TouchpadController touchpad;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi { get; }

		/// <summary>
		/// Gets the base board / notebook model name.
		/// </summary>
		public string BaseBoard => this.Wmi.BaseBoard;

		/// <summary>
		/// Gets the SKU name of the notebook.
		/// </summary>
		public string Sku => this.Wmi.Sku;

		/// <summary>
		/// Gets the serial number. Should match the one found on the underside of the notebook.
		/// </summary>
		public string SerialNumber => this.Wmi.SerialNumber;

		/// <summary>
		/// Gets the BIOS version strings.
		/// </summary>
		public ImmutableArray<string> BiosVersions => this.Wmi.BiosVersions;

		/// <summary>
		/// Gets the CPU controller.
		/// </summary>
		public ICpuController Cpu => this.cpu ?? (this.cpu = new WmiCpuController(this.Wmi));

		/// <summary>
		/// Gets the GPU controller.
		/// </summary>
		public IGpuController Gpu
		{
			get
			{
				if (this.gpu == null)
				{
					NVIDIA.Initialize();
					PhysicalGPU physicalGpu = PhysicalGPU.GetPhysicalGPUs().FirstOrDefault();

					if (this.Sku.StartsWith("P75"))
					{
						this.gpu = new Aero2019GpuController(physicalGpu, this.Wmi);
					}
					else
					{
						this.gpu = new NvGpuController(physicalGpu);
					}
				}

				return this.gpu;
			}
		}

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyboardController Keyboard => this.keyboard ?? (this.keyboard = new KeyboardController());

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public IFanController Fans
		{
			get
			{
				if (this.fans == null)
				{
					if (this.Sku.StartsWith("P75"))
					{
						this.fans = new Aero2019FanController(this.Wmi);
					}
					else
					{
						this.fans = new Aero15Xv8FanController(this.Wmi);
					}
				}

				return this.fans;
			}
		}

		/// <summary>
		/// Gets the screen controller.
		/// </summary>
		public DisplayController Display => this.display ?? (this.display = new DisplayController(this.Wmi));

		/// <summary>
		/// Gets the battery stats / controller.
		/// </summary>
		public BatteryController Battery => this.battery ?? (this.battery = new BatteryController(this.Wmi));

		/// <summary>
		/// Gets the touchpad controller.
		/// </summary>
		public TouchpadController Touchpad => this.touchpad ?? (this.touchpad = new TouchpadController(this.Wmi));

		/// <summary>
		/// Gets or sets the software wifi enable state.
		/// </summary>
		public bool WifiEnabled
		{
			get
			{
				using (var wl = new WlanClient())
				{
					Wlan.Dot11RadioState state = wl.Interfaces.FirstOrDefault()?.RadioState.PhyRadioState.FirstOrDefault().dot11SoftwareRadioState ?? Wlan.Dot11RadioState.Unknown;
					return state == Wlan.Dot11RadioState.On;
				}
			}
			set
			{
				using (var wl = new WlanClient())
				{
					Wlan.WlanPhyRadioState newState;
					if (value)
					{
						newState = new Wlan.WlanPhyRadioState
						{
							dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
							dot11SoftwareRadioState = Wlan.Dot11RadioState.On,
						};
					}
					else
					{
						newState = new Wlan.WlanPhyRadioState
						{
							dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
							dot11SoftwareRadioState = Wlan.Dot11RadioState.Off,
						};
					}

					foreach (var iface in wl.Interfaces)
					{
						iface.SetRadioState(newState);
					}
				}
			}
		}

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.Wmi = wmi;
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			this.Wmi?.Dispose();
			this.Keyboard?.Dispose();
		}

		#endregion
	}
}
