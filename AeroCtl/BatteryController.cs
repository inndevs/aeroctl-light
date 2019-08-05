using System;

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

		public int BatteryHealth => this.wmi.InvokeGet<byte>("GetBatteryHealth");

		public BatteryController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}
	}
}
