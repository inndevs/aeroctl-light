using System;

namespace AeroCtl.Native
{
	internal struct DEVPROPKEY
	{
		public static readonly DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc = new DEVPROPKEY
		{
			fmtid = new Guid("{a45c254e-df1c-4efd-8020-67d146a850e0}"),
			pid = 2
		};

		public static readonly DEVPROPKEY DEVPKEY_Device_Parent = new DEVPROPKEY
		{
			fmtid = new Guid("{4340A6C5-93FA-4706-972C-7B648008A5A7}"),
			pid = 8
		};

		public static readonly DEVPROPKEY DEVPKEY_Device_Children = new DEVPROPKEY
		{
			fmtid = new Guid("{4340A6C5-93FA-4706-972C-7B648008A5A7}"),
			pid = 9
		};

		public Guid fmtid;
		public uint pid;
	}
}