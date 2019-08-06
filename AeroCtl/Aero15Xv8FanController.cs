using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

		public int Rpm1
		{
			get => this.wmi.InvokeGet<ushort>("getRpm1");
		}

		public int Rpm2
		{
			get => this.wmi.InvokeGet<ushort>("getRpm2");
		}

		public void SetQuiet()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = false;
			SmartCoolingStatus = true;
		}

		public void SetNormal()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = false;
		}

		public void SetGaming()
		{
			FanFixedStatus = false;
			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			CurrentFanStep = 0;
			AutoFanStatus = true;
		}

		public void SetFixed(double fanSpeed = 0.25)
		{
			Debug.Assert(fanSpeed >= 0.0 && fanSpeed <= 1.0);

			FanSpeed = false;
			SmartCoolingStatus = false;
			StepFanStatus = false;
			AutoFanStatus = false;
			CurrentFanStep = 0;
			FanFixedStatus = true;
			FixedFanSpeed = (byte)Math.Round(fanSpeed * 229.0);
		}

		public void SetAuto(double fanAdjust = 0.25)
		{
			throw new NotSupportedException();
		}

		public void SetCustom()
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
