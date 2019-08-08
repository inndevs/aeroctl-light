using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AeroCtl.Native
{
	public enum HidUsagePage : ushort
	{
		UNDEFINED = 0,
		GENERIC = 1,
		SIMULATION = 2,
		VR = 3,
		SPORT = 4,
		GAME = 5,
		KEYBOARD = 7,
	}

	public enum HidUsage : ushort
	{
		Undefined = 0,
		Pointer = 1,
		Mouse = 2,
		Joystick = 4,
		Gamepad = 5,
		Keyboard = 6,
		Keypad = 7,
		Consumer = 12, // 0x000C
		SystemControl = 128, // 0x0080
		Tablet = 128, // 0x0080
	}

	public class Hid
	{
		private const string lib = "hid.dll";

		public const int HIDP_STATUS_SUCCESS = 0x11 << 16;

		[DllImport(lib, SetLastError = true)]
		public static extern void HidD_GetHidGuid(out Guid hidGuid);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_GetAttributes(
			SafeFileHandle HidDeviceObject,
			ref HIDD_ATTRIBUTES Attributes);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_GetSerialNumberString(
			SafeFileHandle HidDeviceObject,
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder Buffer,
			uint BufferLength);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_GetPreparsedData(
			SafeFileHandle HidDeviceObject,
			out IntPtr PreparsedData);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

		[DllImport(lib, SetLastError = true)]
		public static extern int HidP_GetCaps(IntPtr preparsedData, out HIDP_CAPS capabilities);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_FlushQueue(SafeFileHandle HidDeviceObject);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_GetFeature(
			SafeFileHandle hidDeviceObject,
			[MarshalAs(UnmanagedType.LPArray)] byte[] lpReportBuffer,
			int reportBufferLength);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_GetFeature(
			SafeFileHandle hidDeviceObject,
			ref byte lpReportBuffer,
			int reportBufferLength);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_SetFeature(
			SafeFileHandle hDevice,
			[MarshalAs(UnmanagedType.LPArray)] byte[] ReportBuffer,
			uint ReportBufferLength);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_SetFeature(
			SafeFileHandle hidDeviceObject,
			ref byte lpReportBuffer,
			int reportBufferLength);
	}
}
