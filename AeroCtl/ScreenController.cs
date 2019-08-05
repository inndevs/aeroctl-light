using AeroCtl.Native;
using System;
using System.Management;

namespace AeroCtl
{
	public class ScreenController
	{
		private readonly AeroWmi wmi;

		public static Guid VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
		public static Guid VIDEO_NORMALLEVEL = new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb");

		public int Brightness
		{
			get
			{
				Guid activePlan = PowerManager.GetActivePlan();
				return (int)PowerManager.GetPlanSetting(activePlan, VIDEO_SUBGROUP, VIDEO_NORMALLEVEL, PowerManager.GetCurrentMode());
			}
			set
			{
				if (value < 0 || value > 100)
					throw new ArgumentOutOfRangeException(nameof(value));

				Guid activePlan = PowerManager.GetActivePlan();
				PowerManager.SetPlanSetting(activePlan, VIDEO_SUBGROUP, VIDEO_NORMALLEVEL, PowerManager.GetCurrentMode(), (uint)value);
				PowerManager.SetActivePlan(activePlan);
			}
		}

		// not functional.
		//public bool Backlight
		//{
		//	get
		//	{
		//		ManagementBaseObject inParams = this.wmi.GetClass.GetMethodParameters("GetBirightnessOff"); // sic
		//		ManagementBaseObject outParams = this.wmi.Get.InvokeMethod("GetBirightnessOff", inParams, null);
		//		return Convert.ToByte(outParams["Data"]) == 0;
		//	}
		//}

		public ScreenController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		public bool ToggleScreen()
		{
			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("SetBrightnessOff");
			inParams["Data"] = (byte)1;

			try
			{
				ManagementBaseObject outParams = this.wmi.Set.InvokeMethod("SetBrightnessOff", inParams, null);
				byte dataOut = Convert.ToByte(outParams["DataOut"]);
				return true;
			}
			catch (ManagementException)
			{
				// Apparently this is expected to throw an exception for whatever reason, even though it does toggle the screen.
			}

			return false;
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
			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("DecreaseBrigtness"); // sic
			inParams["Data"] = (byte)0;
			ManagementBaseObject outParams = this.wmi.Set.InvokeMethod("DecreaseBrigtness", inParams, null);
			return Convert.ToByte(outParams["DataOut"]);
		}

	}
}
