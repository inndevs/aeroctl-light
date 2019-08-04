using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct RAWKEYBOARD
	{
		public ushort MakeCode;
		public ushort Flags;
		public ushort Reserved;
		public ushort VKey;
		public uint Message;
		public uint ExtraInformation;
	}
}