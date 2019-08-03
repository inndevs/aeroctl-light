using System.Runtime.InteropServices;

namespace AeroCtl.Native
{
	internal struct HIDP_CAPS
	{
		public ushort Usage;
		public ushort UsagePage;
		public short InputReportByteLength;
		public short OutputReportByteLength;
		public short FeatureReportByteLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
		public short[] Reserved;

		public short NumberLinkCollectionNodes;
		public short NumberInputButtonCaps;
		public short NumberInputValueCaps;
		public short NumberInputDataIndices;
		public short NumberOutputButtonCaps;
		public short NumberOutputValueCaps;
		public short NumberOutputDataIndices;
		public short NumberFeatureButtonCaps;
		public short NumberFeatureValueCaps;
		public short NumberFeatureDataIndices;
	}
}