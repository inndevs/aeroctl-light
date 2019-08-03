using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct RAWMOUSE
	{
		public const ushort MOUSE_MOVE_RELATIVE = 0;
		public const ushort MOUSE_MOVE_ABSOLUTE = 1;
		public const ushort MOUSE_VIRTUAL_DESKTOP = 2;
		public const ushort MOUSE_ATTRIBUTES_CHANGED = 4;

		[StructLayout(LayoutKind.Explicit)]
		public struct Union
		{
			[FieldOffset(0)]
			public uint ulButtons;
			[FieldOffset(0)]
			public ushort usButtonFlags;
			[FieldOffset(2)]
			public ushort usButtonData;
		}

		public ushort usFlags;
		public Union data;
		public uint ulRawButtons;
		public int lLastX;
		public int lLastY;
		public uint ulExtraInformation;
	}
}