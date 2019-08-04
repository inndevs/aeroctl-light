using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RAWINPUTDEVICE
	{
		public const uint RIDEV_NONE = 0x0000;
		public const uint RIDEV_REMOVE = 0x0001;
		public const uint RIDEV_EXCLUDE = 0x0010;
		public const uint RIDEV_PAGEONLY = 0x0020;
		public const uint RIDEV_NOLEGACY = 0x0030;
		public const uint RIDEV_INPUTSINK = 0x0100;
		public const uint RIDEV_CAPTUREMOUSE = 0x0200;
		public const uint RIDEV_NOHOTKEYS = 0x0200;
		public const uint RIDEV_APPKEYS = 0x0400;
		public const uint RIDEV_EXINPUTSINK = 0x1000;
		public const uint RIDEV_DEVNOTIFY = 0x2000;

		public ushort usUsagePage;
		public ushort usUsage;
		public uint dwFlags;
		public IntPtr hwndTarget;
	}
}