using System;
using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class BluetoothController
	{
		private readonly AeroWmi wmi;

		public BluetoothController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public async Task<bool> GetEnabledAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetBluetooth") == 1;
		}

		public async Task SetEnabledAsync(bool enabled)
		{
			await this.wmi.InvokeSetAsync("SetBluetooth", enabled ? (byte)1 : (byte)0);
			try
			{
				await this.wmi.InvokeSetAsync("SetBluetoothLED", enabled ? (byte) 1 : (byte) 0);
			}
			catch (ManagementException) // Always thrown by design.
			{ }
		}
	}
}