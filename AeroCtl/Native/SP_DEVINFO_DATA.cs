using System;

namespace AeroCtl.Native
{
	internal struct SP_DEVINFO_DATA
	{
		public uint cbSize;
		public Guid ClassGuid;
		public uint DevInst;
		public IntPtr Reserved;
	}
}