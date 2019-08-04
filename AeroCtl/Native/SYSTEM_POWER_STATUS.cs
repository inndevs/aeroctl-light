using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[Flags]
	public enum BatteryChargeStatus : byte
	{
		High = 1,
		Low = 2,
		Critical = 4,
		Charging = 8,
		NoSystemBattery = 128, // 0x80
		Unknown = 255, // 0xFF
	}

	[Flags]
	public enum PowerLineStatus : byte
	{
		Offline = 0,
		Online = 1,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SYSTEM_POWER_STATUS
	{
		public PowerLineStatus ACLineStatus;
		public BatteryChargeStatus BatteryFlag;
		public byte BatteryLifePercent;
		public byte SystemStatusFlag;
		public int BatteryLifeRemaining;
		public int BatteryFullLifeTime;
	}
}
