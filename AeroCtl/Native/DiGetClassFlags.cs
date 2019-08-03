using System;

namespace AeroCtl.Native
{
	[Flags]
	internal enum DiGetClassFlags
	{
		Default = 1,
		Present = 2,
		AllClasses = 4,
		Profile = 8,
		DeviceInterface = 16, // 0x00000010
	}
}