﻿using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Controller for the newer Aero models that expose additional GPU settings.
	/// </summary>
	public class P7GpuController : NvGpuController
	{
		private readonly AeroWmi wmi;

		public P7GpuController(AeroWmi wmi)
		{
			this.wmi = wmi;
			this.PowerConfigSupported = this.wmi.HasMethod("GetNvPowerConfig");
			this.DynamicBoostSupported = this.wmi.HasMethod("GetDynamicBoostStatus");
			this.AiBoostSupported = this.wmi.HasMethod("GetAIBoostStatus");
			this.ThermalTargetSupported = this.wmi.HasMethod("GetNvThermalTarget");
		}

		public override async ValueTask<double?> GetTemperatureAsync()
		{
			// Not sure what the difference between these two is. Query both, just in case.
			try
			{
				return await this.wmi.InvokeGetAsync<ushort>("getGpuTemp1");
			}
			catch (ManagementException)
			{
				try
				{
					return await this.wmi.InvokeGetAsync<ushort>("getGpuTemp2");
				}
				catch (ManagementException)
				{
					return null;
				}
			}
		}

		public bool PowerConfigSupported { get; set; }

		public async Task<bool> GetPowerConfigAsync()
		{
			try
			{
				return await this.wmi.InvokeGetAsync<byte>("GetNvPowerConfig") != 0;
			}
			catch (ManagementException)
			{
				this.PowerConfigSupported = false;
				return false;
			}
		}

		public async Task SetPowerConfigAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetNvPowerConfig", value ? (byte)1 : (byte)0);
		}

		public bool DynamicBoostSupported { get; set; }

		public async Task<bool> GetDynamicBoostAsync()
		{
			try
			{
				return await this.wmi.InvokeGetAsync<byte>("GetDynamicBoostStatus") != 1;
			}
			catch (ManagementException)
			{
				this.DynamicBoostSupported = false;
				return false;
			}
		}

		public async Task SetDynamicBoostAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetDynamicBoostStatus", value ? (byte)1 : (byte)0);
		}

		public bool AiBoostSupported { get; set; }

		public async Task<bool> GetAiBoostEnabledAsync()
		{
			try
			{
				return await this.wmi.InvokeGetAsync<byte>("GetAIBoostStatus") != 0;
			}
			catch (ManagementException)
			{
				this.AiBoostSupported = false;
				return false;
			}
		}

		public async Task SetAiBoostEnabledAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetAIBoostStatus", value ? (byte)1 : (byte)0);
		}

		public bool ThermalTargetSupported { get; set; }

		public async Task<bool> GetThermalTargetEnabledAsync()
		{
			try
			{
				return await this.wmi.InvokeGetAsync<byte>("GetNvThermalTarget") != 0;
			}
			catch (ManagementException)
			{
				this.ThermalTargetSupported = false;
				return false;
			}
		}

		public async Task SetThermalTargetEnabledAsync(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", value ? (byte)1 : (byte)0);
		}
	}
}