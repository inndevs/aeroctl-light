using AeroCtl.Native;
using System;
using System.Management;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Controls the built-in display.
	/// </summary>
	public class DisplayController
	{
		private readonly AeroWmi wmi;

		private static readonly Guid VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
		private static readonly Guid VIDEO_NORMALLEVEL = new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb");

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

		public DisplayController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		/// <summary>
		/// Toggles the screen backlight on/off.
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ToggleScreenAsync()
		{
			try
			{
				await this.wmi.InvokeSetAsync<byte>("SetBrightnessOff", 1);
				return true;
			}
			catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidObject)
			{
				// Apparently this is expected to throw an exception for whatever reason, even though it does toggle the screen.
			}

			return false;
		}

		/// <summary>
		/// Gets the lid status.
		/// </summary>
		/// <returns></returns>
		public async Task<LidStatus> GetLidStatus()
		{
			byte val = await this.wmi.InvokeGetAsync<byte>("GetLid1Status");

			if (val == 0)
				return LidStatus.Closed;

			return LidStatus.Open;
		}
	}
}
