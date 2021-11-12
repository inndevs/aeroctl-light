using System.Threading.Tasks;

namespace AeroCtl
{
	public class Aero15Xv8FanController : IFanController
	{
		private readonly AeroWmi wmi;

		public Aero15Xv8FanController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#region IFanController

		public async ValueTask<(int fan1, int fan2)> GetRpmAsync()
		{
			int rpm1 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm1"));
			int rpm2 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm2"));

			return (rpm1, rpm2);
		}

		public async ValueTask<double> GetPwmAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetFanPWMStatus") / 229.0;
		}

		#endregion

		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}
	}
	
}