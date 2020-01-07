using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public struct POINTL
	{
		[MarshalAs(UnmanagedType.I4)]
		public int x;
		[MarshalAs(UnmanagedType.I4)]
		public int y;
	}
}