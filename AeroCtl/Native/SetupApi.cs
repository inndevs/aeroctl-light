using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	public class SetupApi
	{
		private const string lib = "setupapi.dll";

		[DllImport(lib, CharSet = CharSet.Auto)]
		public static extern IntPtr SetupDiGetClassDevs(
			ref Guid classGuid,
			IntPtr enumerator,
			IntPtr hwndParent,
			DiGetClassFlags flags);

		[DllImport(lib, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
			uint deviceInterfaceDetailDataSize,
			out uint requiredSize,
			ref SP_DEVINFO_DATA deviceInfoData);

		[DllImport(lib, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			ref SP_DEVINFO_DATA devInfo,
			ref Guid interfaceClassGuid,
			uint memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport(lib, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			IntPtr devInfo,
			ref Guid interfaceClassGuid,
			uint memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport(lib, SetLastError = true)]
		public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
	}
}
