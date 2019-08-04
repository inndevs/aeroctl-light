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
		public int Temperature { get; set; }

		/// <summary>
		/// Gets or sets the fan speed (Y coordinate).
		/// </summary>
		public int Speed { get; set; }

		public bool Equals(FanPoint other)
		{
			return Temperature == other.Temperature && Speed == other.Speed;
		}

		public override bool Equals(object obj)
		{
			return obj is FanPoint other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this.Temperature * 397) ^ this.Speed;
			}
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