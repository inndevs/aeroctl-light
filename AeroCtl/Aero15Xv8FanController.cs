using System;
using System.Diagnostics;
using System.Management;
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

		public async Task<bool> GetAutoFanStatus()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetAutoFanStatus") != 0;
		}

		public async Task SetAutoFanStatus(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", value ? (byte)1 : (byte)0);
		}

		public async Task<bool> GetFanFixedStatus()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetFanFixedStatus") != 0;
		}

		public async Task SetFanFixedStatus(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", value ? (byte)1 : (byte)0);
		}

		public async Task SetFixedFanSpeed(byte value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanSpeed", value);
		}

		public async Task<bool> GetFanSpeed()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetFanSpeed") != 0;
		}

		public async Task SetFanSpeed(bool value)
		{
			try
			{
				await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", value ? (byte) 1 : (byte) 0);
			}
			catch (ManagementException) // Always thrown by design.
			{ }
		}

		public async Task SetCurrentFanStep(byte value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetCurrentFanStep", value);
		}

		public async Task<bool> GetStepFanStatus()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetStepFanStatus") != 0;
		}

		public async Task SetStepFanStatus(bool value)
		{
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", value ? (byte)1 : (byte)0);
		}

		public async Task<bool> GetSmartCoolingStatus()
		{
			return await this.wmi.InvokeGetAsync<byte>("GetSmartCool") != 0;
		}

		public async Task SetSmartCoolingStatus(bool value)
		{
			try
			{
				await this.wmi.InvokeSetAsync<byte>("SetSmartCool", value ? (byte) 1 : (byte) 0);
			}
			catch (ManagementException) // Always thrown by design.
			{ }
		}

		#region IFanController

		public async Task<(int fan1, int fan2)> GetRpmAsync()
		{
			int rpm1 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm1"));
			int rpm2 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm2"));

			return (rpm1, rpm2);
		}

		public async Task SetQuietAsync()
		{
			await SetFanFixedStatus(false);
			await SetFanSpeed(false);
			await SetStepFanStatus(false);
			await SetCurrentFanStep(0);
			await SetAutoFanStatus(false);
			await SetSmartCoolingStatus(true);
		}

		public async Task SetNormalAsync()
		{
			await SetFanFixedStatus(false);
			await SetFanSpeed(false);
			await SetStepFanStatus(false);
			await SetCurrentFanStep(0);
			await SetAutoFanStatus(false);
			await SetSmartCoolingStatus(false);
		}

		public async Task SetGamingAsync()
		{
			await SetFanFixedStatus(false);
			await SetFanSpeed(false);
			await SetStepFanStatus(false);
			await SetCurrentFanStep(0);
			await SetAutoFanStatus(true);
			await SetSmartCoolingStatus(false);
		}

		public async Task SetFixedAsync(double fanSpeed = 0.25)
		{
			Debug.Assert(fanSpeed >= 0.0 && fanSpeed <= 1.0);

			await SetFanFixedStatus(true);
			await SetFanSpeed(false);
			await SetStepFanStatus(false);
			await SetCurrentFanStep(0);
			await SetAutoFanStatus(false);
			await SetSmartCoolingStatus(true);
			await SetFixedFanSpeed((byte)Math.Round(fanSpeed * 229.0));
		}

		public Task SetAutoAsync(double fanAdjust = 0.25)
		{
			throw new NotSupportedException();
		}

		public Task SetCustomAsync()
		{
			throw new NotImplementedException();
		}

		public FanCurve GetFanCurve()
		{
			throw new NotImplementedException();
		}

		#endregion
		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}
	}
}
