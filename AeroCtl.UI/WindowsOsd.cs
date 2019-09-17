using System;
using System.Threading.Tasks;
using AeroCtl.Native;

namespace AeroCtl.UI
{
	/// <summary>
	/// Implements methods to control the Windows OSD.
	/// </summary>
	internal static class WindowsOsd
	{
		private static IntPtr getOsdWindow()
		{
			return User32.FindWindow("NativeHWNDHost", "");
		}

		/// <summary>
		/// Tries to get the Windows OSD host window.
		/// </summary>
		public static async ValueTask<IntPtr> FindOsdWindowAsync()
		{
			IntPtr hWnd = getOsdWindow();
			if (hWnd != IntPtr.Zero)
				return hWnd;

			// The OSD window doesn't exist when it was never used or explorer.exe isn't running,
			// so we emulate mute/unmute keypress here to force it to show up. It'll be overridden by
			// whatever event follows so it should only be visible for 1 frame or so.

			INPUT[] inputs = new INPUT[2];

			inputs[0].type = User32.INPUT_KEYBOARD;
			inputs[1].type = User32.INPUT_KEYBOARD;

			inputs[0].U.keyboard.wVk = User32.VK_VOLUME_MUTE;
			inputs[1].U.keyboard.wVk = User32.VK_VOLUME_MUTE;

			inputs[1].U.keyboard.dwFlags = User32.KEYEVENTF_KEYUP;

			for (int i = 0; i < 3; ++i)
			{
				User32.SendInput(2, inputs, INPUT.Size);
				await Task.Delay(1);

				User32.SendInput(2, inputs, INPUT.Size);
				await Task.Delay(1);

				hWnd = getOsdWindow();
				if (hWnd != IntPtr.Zero)
					return hWnd;

				await Task.Delay(10);
			}

			// Give up.
			return IntPtr.Zero;
		}

		/// <summary>
		/// Shows the standard Windows brightness slider OSD.
		/// </summary>
		/// <returns></returns>
		public static async ValueTask<bool> ShowBrightnessAsync()
		{
			IntPtr hWnd = await FindOsdWindowAsync();
			if (hWnd == IntPtr.Zero)
				return false;

			uint msg = User32.RegisterWindowMessage("SHELLHOOK");
			return User32.PostMessage(hWnd, msg, new IntPtr(0x37), IntPtr.Zero);
		}

	}
}
