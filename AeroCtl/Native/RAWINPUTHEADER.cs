using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RAWINPUTHEADER
	{
		public const int RIM_TYPEMOUSE = 0;
		public const int RIM_TYPEKEYBOARD = 1;
		public const int RIM_TYPEHID = 2;

		public int dwType;
		public int dwSize;
		public IntPtr hDevice;
		public IntPtr wParam;
	}
}