using AeroCtl.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
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

		private static IEnumerable<DISPLAY_DEVICE> enumDisplayDevices()
		{
			for (uint i = 0;; ++i)
			{
				DISPLAY_DEVICE dev = default;
				dev.cb = Marshal.SizeOf<DISPLAY_DEVICE>();

				if (!User32.EnumDisplayDevices(null, i, ref dev, 0))
					break;

				yield return dev;
			}
		}

		/// <summary>
		/// Returns the name of the integrated display, if it is connected.
		/// </summary>
		/// <returns>The device name of the integrated display, or null if not connected.</returns>
		public string GetIntegratedDisplayName()
		{
			return enumDisplayDevices()
				.Where(d => (d.StateFlags & DisplayDeviceStateFlags.Remote) == 0)
				.Where(d => d.DeviceID.Contains("VEN_8086"))
				.OrderBy(d => d.DeviceName)
				.FirstOrDefault().DeviceName;
		}

		/// <summary>
		/// Returns the current frequency of the integrated display.
		/// </summary>
		/// <returns></returns>
		public uint? GetIntegratedDisplayFrequency()
		{
			string devName = this.GetIntegratedDisplayName();
			if (devName == null)
				return null;

			DEVMODE current = default;
			current.dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
			if (!User32.EnumDisplaySettings(devName, User32.ENUM_CURRENT_SETTINGS, ref current))
				return null;

			return current.dmDisplayFrequency;
		}

		/// <summary>
		/// Enumerates the supported display frequencies of the built-in display.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<uint> GetIntegratedDisplayFrequencies()
		{
			string devName = this.GetIntegratedDisplayName();
			if (devName == null)
				yield break;

			DEVMODE current = default;
			current.dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
			if (!User32.EnumDisplaySettings(devName, User32.ENUM_CURRENT_SETTINGS, ref current))
				yield break;

			HashSet<uint> returnedHz = new HashSet<uint>();

			for (int j = 0;; ++j)
			{
				DEVMODE mode = default;
				mode.dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
				if (!User32.EnumDisplaySettings(devName, j, ref mode))
					break;

				if (mode.dmPelsWidth == current.dmPelsWidth && mode.dmPelsHeight == current.dmPelsHeight)
				{
					if (returnedHz.Add(mode.dmDisplayFrequency))
						yield return mode.dmDisplayFrequency;
				}
			}
		}

		/// <summary>
		/// Changes the display frequency of the built-in display.
		/// </summary>
		/// <param name="newFreq"></param>
		/// <returns></returns>
		public bool SetIntegratedDisplayFrequency(uint newFreq)
		{
			string devName = this.GetIntegratedDisplayName();
			if (devName == null)
				return false;

			DEVMODE current = default;
			current.dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
			if (!User32.EnumDisplaySettings(devName, User32.ENUM_CURRENT_SETTINGS, ref current))
				return false;

			current.dmDisplayFrequency = newFreq;
			User32.ChangeDisplaySettings(ref current, 0);

			return true;
		}
	}
}
