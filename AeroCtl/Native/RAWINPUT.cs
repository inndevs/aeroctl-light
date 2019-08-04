using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RAWINPUT
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct Union
		{
			[FieldOffset(0)]
			public RAWMOUSE mouse;
			[FieldOffset(0)]
			public RAWKEYBOARD keyboard;
			[FieldOffset(0)]
			public RAWHID hid;

		}

		public RAWINPUTHEADER header;
		public Union data;
	}
}