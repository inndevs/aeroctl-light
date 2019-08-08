using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NvAPIWrapper.GPU;

namespace AeroCtl
{
	/// <summary>
	/// Controller for the newer Aero models that expose additional GPU settings.
	/// </summary>
	public class Aero2019GpuController : NvGpuController
	{
		private readonly AeroWmi wmi;

		public Aero2019GpuController(PhysicalGPU gpu, AeroWmi wmi) 
			: base(gpu)
		{
			this.wmi = wmi;
		}

		public async Task<bool> GetPowerConfigAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetNvPowerConfig") != 0;
		}

		public async Task SetPowerConfigAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetNvPowerConfig", value ? (byte)1 : (byte)0);
		}

		public async Task<bool> GetBoostEnabledAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetAIBoostStatus") != 0;
		}

		public async Task SetBoostEnabledAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetAIBoostStatus", value ? (byte)1 : (byte)0);
		}
	}
}