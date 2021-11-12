using System;
using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Implements the fan controller and thermal management of the notebook.
	/// </summary>
	public class P7FanController : IFanController, IFanControllerSync
	{
		#region Fields

		private readonly AeroWmi wmi;

		private const int minFanSpeed = 0;
		private const int maxFanSpeed = 229;

		#endregion

		#region Constructors

		public P7FanController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#endregion

		#region Methods

		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}

		public async ValueTask<(int fan1, int fan2)> GetRpmAsync()
		{
			int rpm1 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm1"));
			int rpm2 = reverse(await this.wmi.InvokeGetAsync<ushort>("getRpm2"));

			return (rpm1, rpm2);
		}

		public async ValueTask<double> GetPwmAsync()
		{
			return absToRel(await this.wmi.InvokeGetAsync<byte>("GetFanPWMStatus"));
		}
		
		public void SetFixed(double fanSpeed = 0.25)
		{
			this.wmi.InvokeSet<byte>("SetAutoFanStatus", 0);
			this.wmi.InvokeSet<byte>("SetStepFanStatus", 1);
			this.wmi.InvokeSet<byte>("SetFixedFanStatus", 1);
			this.wmi.InvokeSet<byte>("SetFixedFanSpeed", (byte)relToAbs(fanSpeed));
			this.wmi.InvokeSet<byte>("SetGPUFanDuty", (byte)relToAbs(fanSpeed)); // Only available on some models (?)
		}

		private static int relToAbs(double fanSpeed)
		{
			if (fanSpeed <= 0.0)
				return minFanSpeed;
			if (fanSpeed >= 1.0)
				return maxFanSpeed;

			return (int) (minFanSpeed + fanSpeed * (maxFanSpeed - minFanSpeed));
		}

		private static double absToRel(int fanSpeed)
		{
			return (double) (fanSpeed - minFanSpeed) / (maxFanSpeed - minFanSpeed);
		}

		#endregion
	}
}