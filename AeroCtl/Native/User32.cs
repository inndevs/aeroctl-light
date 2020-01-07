using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct HARDWAREINPUT
	{
		public int uMsg;
		public short wParamL;
		public short wParamH;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct KEYBDINPUT
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public int time;
		public UIntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public int mouseData;
		public uint dwFlags;
		public uint time;
		public UIntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct INPUT
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct InputUnion
		{
			[FieldOffset(0)] public MOUSEINPUT mouse;
			[FieldOffset(0)] public KEYBDINPUT keyboard;
			[FieldOffset(0)] public HARDWAREINPUT hardware;
		}
		
		public uint type;
		public InputUnion U;

		public static int Size => Marshal.SizeOf(typeof(INPUT));
	}

	public static class User32
	{
		private const string lib = "user32.dll";

		public const uint RID_INPUT = 0x10000003;
		public const uint RID_HEADER = 0x10000005;

		public const ushort VK_VOLUME_MUTE = 0xAD;

		public const uint INPUT_MOUSE = 0;
		public const uint INPUT_KEYBOARD = 1;

		public const uint KEYEVENTF_KEYUP = 2;

		public const int ENUM_CURRENT_SETTINGS = -1;

		public const int DISP_CHANGE_SUCCESSFUL = 0;
		public const int DISP_CHANGE_RESTART = 1;
		public const int DISP_CHANGE_FAILED = -1;
		public const int DISP_CHANGE_BADMODE = -2;
		public const int DISP_CHANGE_NOTUPDATED = -3;
		public const int DISP_CHANGE_BADFLAGS = -4;
		public const int DISP_CHANGE_BADPARAM = -5;

		[DllImport(lib, SetLastError = true)]
		public static extern bool RegisterRawInputDevices(
			RAWINPUTDEVICE[] pRawInputDevice,
			uint numberDevices,
			uint size);

		[DllImport(lib, SetLastError = true)]
		public static extern int GetRawInputData(
			IntPtr hRawInput,
			uint command,
			out RAWINPUT pData,
			[In, Out] ref int pcbSize,
			int cbSizeHeader);

		[DllImport(lib, SetLastError = true)]
		public static extern int GetRawInputData(
			IntPtr hRawInput,
			uint command,
			IntPtr pData,
			[In, Out] ref int pcbSize,
			int cbSizeHeader);

		[DllImport(lib, SetLastError = true)]
		public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

		[DllImport(lib, SetLastError = true)]
		public static extern IntPtr RegisterPowerSettingNotification(
			IntPtr hRecipient,
			ref Guid PowerSettingGuid,
			uint Flags);

		[DllImport(lib, SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport(lib, SetLastError = true)]
		public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport(lib, SetLastError = true)]
		public static extern uint RegisterWindowMessage(string lpString);

		[DllImport(lib, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumDisplaySettings(
			[param: MarshalAs(UnmanagedType.LPTStr)]
			string lpszDeviceName,
			int iModeNum,
			[In, Out] ref DEVMODE lpDevMode);

		[DllImport(lib, SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.I4)]
		public static extern int ChangeDisplaySettings([In, Out] ref DEVMODE lpDevMode,
			[param: MarshalAs(UnmanagedType.U4)] uint dwflags);

		[DllImport(lib, SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, [In, Out] ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
	}
}