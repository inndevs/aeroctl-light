using System;
using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Implements the fan controller and thermal management of the notebook.
	/// </summary>
	public class P75FanController : IFanController, IDirectFanSpeedController
	{
		#region Fields

		private readonly AeroWmi wmi;

		private const int minFanSpeed = 0;
		private const int maxFanSpeed = 229;
		private const int fanCurvePointCount = 15;

		#endregion

		#region Constructors

		public P75FanController(AeroWmi wmi)
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

		public async ValueTask SetQuietAsync()
		{
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 0);
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 1);
		}

		public async ValueTask SetNormalAsync()
		{
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 0);
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 0);
		}

		public async ValueTask SetGamingAsync()
		{
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 0);
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 1);
			await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 0);
		}

		public async ValueTask SetAutoAsync(double fanAdjust = 0.25)
		{
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 1);
			//await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 0);
			await this.wmi.InvokeSetAsync<byte>("SetFanAdjustStatus", (byte)relToAbs(fanAdjust));
		}

		public async ValueTask SetFixedAsync(double fanSpeed = 0.25)
		{
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 1);
			//await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 0);
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 1);
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanSpeed", (byte)relToAbs(fanSpeed));
		}

		public void SetFixed(double fanSpeed = 0.25)
		{
			this.wmi.InvokeSet<byte>("SetAutoFanStatus", 0);
			this.wmi.InvokeSet<byte>("SetStepFanStatus", 1);
			this.wmi.InvokeSet<byte>("SetFixedFanStatus", 1);
			this.wmi.InvokeSet<byte>("SetFixedFanSpeed", (byte)relToAbs(fanSpeed));
		}

		public async ValueTask SetCustomAsync()
		{
			await this.wmi.InvokeSetAsync<byte>("SetAutoFanStatus", 0);
			await this.wmi.InvokeSetAsync<byte>("SetFixedFanStatus", 0);
			// await this.wmi.InvokeSetAsync<byte>("SetFanSpeed", 0);
			await this.wmi.InvokeSetAsync<byte>("SetStepFanStatus", 1);
			//await this.wmi.InvokeSetAsync<byte>("SetNvThermalTarget", 0);
		}

		public FanCurve GetFanCurve()
		{
			return new Curve(this);
		}

		/// <summary>
		/// Returns the fan point at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public FanPoint GetFanCurvePoint(int index)
		{
			if (index < 0 || index >= fanCurvePointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			var res = this.wmi.Invoke("GetFanIndexValue", ("Index", (byte)index));

			return new FanPoint
			{
				Temperature = (byte)res["Temperture"], // sic
				FanSpeed = absToRel((byte)res["Value"]),
			};
		}

		/// <summary>
		/// Sets the fan point at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="point"></param>
		public void SetFanCurvePoint(int index, FanPoint point)
		{
			if (index < 0 || index >= fanCurvePointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			this.wmi.Invoke("SetFanIndexValue",
				("Index", (byte)index), 
				("Temperture", (byte)point.Temperature), // sic
				("Value", (byte)relToAbs(point.FanSpeed)));
		}

		#endregion

		#region Nested Types

		private sealed class Curve : FanCurve
		{
			private readonly P75FanController controller;
			public Curve(P75FanController controller)
			{
				this.controller = controller;
			}


			public override FanPoint this[int index]
			{
				get => this.controller.GetFanCurvePoint(index);
				set => this.controller.SetFanCurvePoint(index, value);
			}

			public override int Count => fanCurvePointCount;
		}

		#endregion


	}
}