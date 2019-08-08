using System;

namespace AeroCtl.UI
{
	public struct FanConfig
	{
		/// <summary>
		/// Time between updates.
		/// </summary>
		public TimeSpan Interval;

		/// <summary>
		/// Maximum fan ramp up speed per second.
		/// </summary>
		public double RampUpSpeed;

		/// <summary>
		/// Maximum fan ramp down speed per second.
		/// </summary>
		public double RampDownSpeed;

		/// <summary>
		/// The fan controller scheduling mode.
		/// </summary>
		public FanSchedulingMode SchedulingMode;

		public static readonly FanConfig Default = new FanConfig
		{
			Interval = TimeSpan.FromSeconds(1.0 / 3.0),
			RampUpSpeed = 0.2,
			RampDownSpeed = 0.03,
			SchedulingMode = FanSchedulingMode.AboveNormalThread
		};
	}
}