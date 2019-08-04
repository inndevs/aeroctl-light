using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct SP_DEVICE_INTERFACE_DETAIL_DATA
	{
		public uint cbSize;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string DevicePath;
	}
}