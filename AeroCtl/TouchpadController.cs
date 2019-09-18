using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class TouchpadController
	{
		private readonly AeroWmi wmi;

		public TouchpadController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public async Task<bool> GetEnabledAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetTouchPad") == 0;
		}

		public async Task SetEnabledAsync(bool enabled)
		{
			await this.wmi.InvokeSetAsync("SetTouchPad", enabled ? (byte)0 : (byte)1);
		}
	}
}