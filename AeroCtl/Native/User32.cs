using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	public class User32
	{
		private const string lib = "user32.dll";
		
		public const uint RID_INPUT = 0x10000003;
		public const uint RID_HEADER = 0x10000005;

		[DllImport(lib, SetLastError = true)]
		internal static extern bool RegisterRawInputDevices(
			RAWINPUTDEVICE[] pRawInputDevice,
			uint numberDevices,
			uint size);

		[DllImport(lib, SetLastError = true)]
		internal static extern int GetRawInputData(
			IntPtr hRawInput,
			uint command,
			out RAWINPUT pData,
			[In, Out] ref int pcbSize,
			int cbSizeHeader);

		[DllImport(lib, SetLastError = true)]
		internal static extern int GetRawInputData(
			IntPtr hRawInput,
			uint command,
			IntPtr pData,
			[In, Out] ref int pcbSize,
			int cbSizeHeader);

		[DllImport(lib, SetLastError = true)]
		public static extern IntPtr RegisterPowerSettingNotification(
		  IntPtr hRecipient,
		  ref Guid PowerSettingGuid,
		  uint Flags);
	}
}