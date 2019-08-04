using System;
using System.Management;

namespace AeroCtl
{
	public class FanController
	{
		#region Fields

		private readonly AeroWmi wmi;

		#endregion

		#region Properties

		public int Rpm1
		{
			get
			{
				ManagementBaseObject inParams = this.wmi.GetClass.GetMethodParameters("getRpm1");
				ManagementBaseObject outParams = this.wmi.Get.InvokeMethod("getRpm1", inParams, null);
				return reverse(Convert.ToUInt16(outParams["Data"]));
			}
		}

		public int Rpm2
		{
			get
			{
				ManagementBaseObject inParams = this.wmi.GetClass.GetMethodParameters("getRpm2");
				ManagementBaseObject outParams = this.wmi.Get.InvokeMethod("getRpm2", inParams, null);
				return reverse(Convert.ToUInt16(outParams["Data"]));
			}
		}

		#endregion

		#region Constructors

		public FanController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#endregion

		#region Methods

		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}

		#endregion
	}
}