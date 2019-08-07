using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class BatteryController
	{
		private readonly AeroWmi wmi;

		public ChargePolicy ChargePolicy
		{
			get => (ChargePolicy) this.wmi.InvokeGet<ushort>("GetChargePolicy");
			set => this.wmi.InvokeSet<byte>("SetChargePolicy", (byte) value);
		}

		public int ChargeStop
		{
			get => this.wmi.InvokeGet<ushort>("GetChargeStop");
			set
			{
				if (value <= 0 || value > 100)
					throw new ArgumentOutOfRangeException(nameof(value));

				this.wmi.InvokeSet<byte>("SetChargeStop", (byte) value);
			}
		}

		public bool SmartCharge
		{
			get => this.wmi.InvokeGet<byte>("GetSmartCharge") != 0;
			set => this.wmi.InvokeSet<byte>("SetSmartCharge", value ? (byte)1 : (byte)0);
		}

		public BatteryController(AeroWmi wmi)
		{
			this.wmi = wmi;
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
