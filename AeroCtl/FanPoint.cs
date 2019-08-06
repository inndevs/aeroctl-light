using System;

namespace AeroCtl
{
	/// <summary>
	/// Represents a point on a fan curve.
	/// </summary>
	public struct FanPoint : IEquatable<FanPoint>
	{
		/// <summary>
		/// Gets or sets the temperature (X coordinate).
		/// </summary>
		public double Temperature { get; set; }

		/// <summary>
		/// Gets or sets the fan speed (Y coordinate).
		/// </summary>
		public double FanSpeed { get; set; }

		public bool Equals(FanPoint other)
		{
			return Temperature == other.Temperature && this.FanSpeed == other.FanSpeed;
		}

		public override bool Equals(object obj)
		{
			return obj is FanPoint other && Equals(other);
		}

		public override int GetHashCode()
		{
			return this.Temperature.GetHashCode() ^ this.FanSpeed.GetHashCode();
		}

		public static bool operator ==(FanPoint left, FanPoint right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FanPoint left, FanPoint right)
		{
			return !left.Equals(right);
		}
	}
}