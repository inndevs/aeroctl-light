using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AeroCtl.Native;
using ManagedNativeWifi;

namespace AeroCtl
{
	// <summary>
	// Implements the AERO interfaces.
	// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private AeroWmi wmi;
		private ICpuController cpu;
		private IGpuController gpu;
		private IFanController fans;
		private BatteryController battery;

		#endregion

		#region Properties

		// Gets the WMI interface.
		private AeroWmi Wmi => this.wmi ?? (this.wmi = new AeroWmi());

		// Gets the base board / notebook model name.
		public string BaseBoard => this.Wmi.BaseBoard;

		// Gets the SKU name of the notebook.
		public string Sku => this.Wmi.Sku;

		// Gets the serial number. Should match the one found on the underside of the notebook.
		public string SerialNumber => this.Wmi.SerialNumber;

		// Gets the BIOS version strings.
		public ImmutableArray<string> BiosVersions => this.Wmi.BiosVersions;

		// Gets the CPU controller.
		public ICpuController Cpu => this.cpu ?? (this.cpu = new WmiCpuController(this.Wmi));

		// Gets the GPU controller.
		public IGpuController Gpu
		{
			get
			{
				if (this.gpu == null)
				{
					if (this.Sku.StartsWith("P7"))
					{
						this.gpu = new P7GpuController(this.Wmi);
					}
					else
					{
						this.gpu = new NvGpuController();
					}
				}

				return this.gpu;
			}
		}

		// Gets the fan controller.
		public IFanController Fans
		{
			get
			{
				if (this.fans == null)
				{
					if (this.Sku.StartsWith("P7"))
					{
						this.fans = new P7FanController(this.Wmi);
					}
					else
					{
						this.fans = new Aero15Xv8FanController(this.Wmi);
					}
				}

				return this.fans;
			}
		}

		// Gets the battery stats / controller.
		public BatteryController Battery => this.battery ?? (this.battery = new BatteryController(this.Wmi));

		#endregion

		#region Methods

		public void Dispose()
		{
			this.Wmi?.Dispose();
		}

		#endregion
	}
	
}
