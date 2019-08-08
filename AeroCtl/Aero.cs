using NativeWifi;
using System;
using System.Collections.Generic;
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

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi { get; }

		/// <summary>
		/// Gets the CPU controller.
		/// </summary>
		public ICpuController Cpu { get; }
		
		/// <summary>
		/// Gets the GPU controller.
		/// </summary>
		public IGpuController Gpu { get; }

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
		public IReadOnlyList<string> BiosVersions => this.Wmi.BiosVersions;

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyboardController Keyboard { get; }

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public IFanController Fans { get; }

		/// <summary>
		/// Gets the screen controller.
		/// </summary>
		public DisplayController Screen { get; }

		/// <summary>
		/// Gets the battery stats / controller.
		/// </summary>
		public BatteryController Battery { get; }

		/// <summary>
		/// Gets the touchpad controller.
		/// </summary>
		public TouchpadController Touchpad { get; }

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

			this.Cpu = new WmiCpuController(wmi);

			NVIDIA.Initialize();
			PhysicalGPU gpu = PhysicalGPU.GetPhysicalGPUs().FirstOrDefault();
			
			if (this.Sku.StartsWith("P75"))
			{ 
				this.Fans = new Aero2019FanController(wmi);
				this.Gpu = new Aero2019GpuController(gpu, wmi);
			}
			else
			{
				this.Fans = new Aero15Xv8FanController(wmi);
				this.Gpu = new NvGpuController(gpu);
			}

			this.Keyboard = new KeyboardController();
			this.Screen = new DisplayController(wmi);
			this.Battery = new BatteryController(wmi);
			this.Touchpad = new TouchpadController(wmi);
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
