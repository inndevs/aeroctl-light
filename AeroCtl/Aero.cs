using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AeroCtl.Native;
using ManagedNativeWifi;

namespace AeroCtl
{
	/// <summary>
	/// Implements the AERO interfaces.
	/// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private AeroWmi wmi;
		private ICpuController cpu;
		private IGpuController gpu;
		private IFanController fans;
		private KeyboardController keyboard;
		private BatteryController battery;
		private DisplayController display;
		private TouchpadController touchpad;
		private BluetoothController bluetooth;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi => this.wmi ?? (this.wmi = new AeroWmi());

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
					if (this.Sku.StartsWith("P75"))
					{
						this.gpu = new P75GpuController(this.Wmi);
					}
					else
					{
						this.gpu = new NvGpuController();
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
						this.fans = new P75FanController(this.Wmi);
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
		public TouchpadController Touchpad => this.touchpad ?? (this.touchpad = new TouchpadController());

		/// <summary>
		/// Gets the Bluetooth controller.
		/// </summary>
		public BluetoothController Bluetooth => this.bluetooth ?? (this.bluetooth = new BluetoothController(this.Wmi));

		#endregion

		#region Constructors

		public Aero()
		{
			
		}

		#endregion

		#region Methods

		public ValueTask<bool?> GetWifiEnabledAsync()
		{
			var targetInterface = NativeWifi.EnumerateInterfaces().FirstOrDefault();
			if (targetInterface == null)
				return new ValueTask<bool?>((bool?)null);

			var radioSet = NativeWifi.GetInterfaceRadio(targetInterface.Id)?.RadioSets.FirstOrDefault();
			return new ValueTask<bool?>(radioSet?.SoftwareOn);
		}

		public async ValueTask SetWifiEnabledAsync(bool enabled)
		{
			foreach (var iface in NativeWifi.EnumerateInterfaces())
			{
				var radioSet = NativeWifi.GetInterfaceRadio(iface.Id)?.RadioSets.FirstOrDefault();
				if (radioSet == null)
					continue;

				if (radioSet.HardwareOn != true)
					continue;

				if (enabled)
					await Task.Run(() => NativeWifi.TurnOnInterfaceRadio(iface.Id));
				else
					await Task.Run(() => NativeWifi.TurnOffInterfaceRadio(iface.Id));
			}
		}

		public void Dispose()
		{
			this.Wmi?.Dispose();
			this.Keyboard?.Dispose();
			this.Touchpad?.Dispose();
		}

		#endregion
	}
}
