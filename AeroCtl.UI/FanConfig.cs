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

		public static readonly FanConfig Default = new FanConfig
		{
			Interval = TimeSpan.FromSeconds(0.75),
			RampUpSpeed = 0.2,
			RampDownSpeed = 0.03,
		};
	}
}