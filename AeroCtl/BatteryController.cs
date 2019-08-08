using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class BatteryController
	{
		private readonly AeroWmi wmi;

		public BatteryController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public async Task<ChargePolicy> GetChargePolicyAsync()
		{
			return (ChargePolicy)await this.wmi.InvokeGetAsync<ushort>("GetChargePolicy");
		}

		public async Task SetChargePolicyAsync(ChargePolicy policy)
		{
			await this.wmi.InvokeSetAsync<byte>("SetChargePolicy", (byte)policy);
		}

		public async Task<int> GetChargeStopAsync()
		{
			return await this.wmi.InvokeGetAsync<ushort>("GetChargeStop");
		}

		public async Task SetChargeStopAsync(int percent)
		{
			if (percent <= 0 || percent > 100)
				throw new ArgumentOutOfRangeException(nameof(percent));

			await this.wmi.InvokeSetAsync<byte>("SetChargeStop", (byte)percent);
		}

		public async Task<bool> GetSmartChargeAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetSmartCharge") != 0;
		}

		public async Task SetSmargeChargeAsync(bool enabled)
		{
			await this.wmi.InvokeSetAsync<byte>("SetSmartCharge", enabled ? (byte)1 : (byte)0);
		}

		public async Task<int> GetRemainingChargeAsync()
		{
			return await Task.Run(() =>
			{
				ManagementObject batt = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Battery")
					.Get()
					.OfType<ManagementObject>()
					.FirstOrDefault();

				if (batt == null)
					return 0;

				return (ushort)batt["EstimatedChargeRemaining"];
			});
		}
		
		public async Task<int> GetHealthAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetBatteryHealth");
		}

		public async Task<int> GetCyclesAsync()
		{
			return await this.wmi.InvokeGetAsync<ushort>("getBattCyc1");
		}
	}
}
