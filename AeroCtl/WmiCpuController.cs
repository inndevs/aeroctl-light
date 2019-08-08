using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// CPU control exposed through the Aero WMI interface.
	/// </summary>
	public class WmiCpuController : ICpuController
	{
		private readonly AeroWmi wmi;

		public WmiCpuController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public async ValueTask<double> GetTemperatureAsync()
		{
			return await this.wmi.InvokeGetAsync<ushort>("getCpuTemp");
		}
	}
}
