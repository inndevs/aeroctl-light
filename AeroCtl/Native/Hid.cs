﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

		[DllImport(lib, SetLastError = true)]
		public static extern void HidD_GetHidGuid(ref Guid hidGuid);

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
			ref IntPtr PreparsedData);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);

		[DllImport(lib, SetLastError = true)]
		public static extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

		[DllImport(lib, SetLastError = true)]
		public static extern bool HidD_FlushQueue(SafeFileHandle HidDeviceObject);


		[DllImport(lib, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern bool HidD_SetFeature(
			SafeFileHandle hDevice,
			byte[] ReportBuffer,
			uint ReportBufferLength);

		[StructLayout(LayoutKind.Sequential)]
		public class SP_DEVINFO_DATA
		{
			public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
			public Guid classGuid = Guid.Empty;
			public int devInst;
			public int reserved;
		}
	}
}