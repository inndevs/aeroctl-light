using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AeroCtl.Native;

namespace DisplayModes
{
	class Program
	{
		static void Main(string[] args)
		{
			for (;;)
			{
				for (uint i = 0;; ++i)
				{
					DISPLAY_DEVICE dev = default;
					dev.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
					if (!User32.EnumDisplayDevices(null, i, ref dev, 0))
						break;

					if ((dev.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)
						continue;

					Console.WriteLine(dev.DeviceID);
					Console.WriteLine(dev.DeviceKey);
					Console.WriteLine(dev.DeviceName);
					Console.WriteLine(dev.DeviceString);
					Console.WriteLine(dev.StateFlags);
					Console.WriteLine();

					for (int j = -1;; ++j)
					{
						DEVMODE mode = default;
						mode.dmSize = (ushort) Marshal.SizeOf<DEVMODE>();
						if (!User32.EnumDisplaySettings(dev.DeviceName, j, ref mode))
							break;

						Console.WriteLine($"{mode.dmDeviceName} {mode.dmPosition.x},{mode.dmPosition.y} {mode.dmFormName} {mode.dmPelsWidth}x{mode.dmPelsHeight}x{mode.dmBitsPerPel} {mode.dmDisplayFrequency}");
					}
				}

				Console.WriteLine("-----");
				Console.ReadKey();
			}
		}
	}
}
