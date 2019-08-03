using System;

namespace AeroCtl.Native
{
	internal struct SP_DEVICE_INTERFACE_DATA
	{
		public uint cbSize;
		public Guid InterfaceClassGuid;
		public uint Flags;
		public UIntPtr Reserved;
	}
}