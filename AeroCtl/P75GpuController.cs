using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Controller for the newer Aero models that expose additional GPU settings.
	/// </summary>
	public class P75GpuController : NvGpuController
	{
		private readonly AeroWmi wmi;

		public P75GpuController(AeroWmi wmi) 
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

		public async Task<bool> GetDynamicBoostAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetDynamicBoostStatus") != 0;
		}

		public async Task SetDynamicBoostAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetDynamicBoostStatus", value ? (byte)1 : (byte)0);
		}

		public async Task<bool> GetAiBoostEnabledAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetAIBoostStatus") != 0;
		}

		public async Task SetAiBoostEnabledAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetAIBoostStatus", value ? (byte)1 : (byte)0);
		}

		public async Task<bool> GetThermalTargetEnabledAsync()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetNvThermalTarget") != 0;
		}

		public async Task SetThermalTargetEnabledAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", value ? (byte)1 : (byte)0);
		}
	}
}