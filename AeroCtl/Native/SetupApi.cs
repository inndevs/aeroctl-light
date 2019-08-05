using System;
using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	public class SetupApi
	{
		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern bool SetupDiEnumDeviceInfo(
			IntPtr DeviceInfoSet,
			uint MemberIndex,
			ref SP_DEVINFO_DATA DeviceInfoData);

		[DllImport("setupapi.dll", EntryPoint = "SetupDiGetDevicePropertyW", SetLastError = true)]
		private static extern bool SetupDiGetDeviceProperty(
			IntPtr DeviceInfoSet,
			ref SP_DEVINFO_DATA DeviceInfoData,
			ref DEVPROPKEY propertyKey,
			out int propertyType,
			IntPtr propertyBuffer,
			int propertyBufferSize,
			out int requiredSize,
			int flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetupDiGetClassDevs(
			ref Guid ClassGuid,
			IntPtr Enumerator,
			IntPtr hwndParent,
			DiGetClassFlags Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
			uint deviceInterfaceDetailDataSize,
			out uint requiredSize,
			ref SP_DEVINFO_DATA deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			ref SP_DEVINFO_DATA devInfo,
			ref Guid interfaceClassGuid,
			uint memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			IntPtr devInfo,
			ref Guid interfaceClassGuid,
			uint memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);



	}
}
