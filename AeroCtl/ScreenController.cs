using AeroCtl.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class ScreenController
	{
		private readonly AeroWmi wmi;

		public static Guid VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
		public static Guid VIDEO_NORMALLEVEL = new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb");

		public uint Brightness
		{
			get
			{
				Guid activePlan = PowerManager.GetActivePlan();
				return PowerManager.GetPlanSetting(activePlan, VIDEO_SUBGROUP, VIDEO_NORMALLEVEL, PowerManager.GetCurrentMode());
			}
			set
			{
				Guid activePlan = PowerManager.GetActivePlan();
				PowerManager.SetPlanSetting(activePlan, VIDEO_SUBGROUP, VIDEO_NORMALLEVEL, PowerManager.GetCurrentMode(), value);
				PowerManager.SetActivePlan(activePlan);
			}
		}

		public ScreenController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public int IncreaseBrightness()
		{
			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("IncreaseBrightness");
			inParams["Data"] = (byte)0;
			ManagementBaseObject outParams = this.wmi.Set.InvokeMethod("IncreaseBrightness", inParams, null);
			return Convert.ToByte(outParams["DataOut"]);
		}

		public int DecreaseBrightness()
		{
			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("DecreaseBrigtness");
			inParams["Data"] = (byte)0;
			ManagementBaseObject outParams = this.wmi.Set.InvokeMethod("DecreaseBrigtness", inParams, null);
			return Convert.ToByte(outParams["DataOut"]);
		}

	}
}
