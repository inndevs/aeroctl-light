using System;
using System.Diagnostics;
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

		public bool AutoFanStatus
		{
			get => this.wmi.InvokeGet<byte>("GetAutoFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetAutoFanStatus", value ? (byte) 1 : (byte) 0);
		}

		public bool FanFixedStatus
		{
			get => this.wmi.InvokeGet<byte>("GetFanFixedStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetFixedFanStatus", value ? (byte)1 : (byte)0);
		}

		public byte FixedFanSpeed
		{
			set => this.wmi.InvokeSet<byte>("SetFixedFanSpeed", value);
		}

		public bool FanSpeed
		{
			get => this.wmi.InvokeGet<byte>("GetFanSpeed") != 0;
			set => this.wmi.InvokeSet<byte>("SetFanSpeed", value ? (byte) 1 : (byte) 0);
		}

		public int CurrentFanStep
		{
			set => this.wmi.InvokeSet<int>("SetCurrentFanStep", value);
		}

		public bool StepFanStatus
		{
			get => this.wmi.InvokeGet<byte>("GetStepFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetStepFanStatus", value ? (byte) 1 : (byte) 0);
		}

		public bool SmartCoolingStatus
		{
			get => this.wmi.InvokeGet<byte>("GetSmartCool") != 0;
			set => this.wmi.InvokeSet<byte>("SetSmartCool", value ? (byte) 1 : (byte) 0);
		}

		#region IFanController

		public async Task<(int fan1, int fan2)> GetRpmAsync()
		{
			int rpm1 = await this.wmi.InvokeGetAsync<ushort>("getRpm1");
			int rpm2 = await this.wmi.InvokeGetAsync<ushort>("getRpm2");

			return (rpm1, rpm2);
		}

		public Task SetQuietAsync()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = false;
			SmartCoolingStatus = true;

			return Task.CompletedTask;
		}

		public Task SetNormalAsync()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = false;

			return Task.CompletedTask;
		}

		public Task SetGamingAsync()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = true;

			return Task.CompletedTask;
		}

		public Task SetFixedAsync(double fanSpeed = 0.25)
		{
			Debug.Assert(fanSpeed >= 0.0 && fanSpeed <= 1.0);

			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			AutoFanStatus = false;
			CurrentFanStep = 0;
			FanFixedStatus = true;
			FixedFanSpeed = (byte)Math.Round(fanSpeed * 229.0);

			return Task.CompletedTask;
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
	}
}
