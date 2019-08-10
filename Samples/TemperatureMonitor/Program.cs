using System;
using System.Linq;
using System.Threading.Tasks;
using AeroCtl;
using OpenHardwareMonitor.Hardware;

namespace TemperatureMonitor
{
	static class Program
	{
		static async Task Main(string[] args)
		{
			bool run = true;
			Console.CancelKeyPress += (s, e) =>
			{
				e.Cancel = true;
				run = false;
			};

			var pc = new Computer {CPUEnabled = true};

			{
				pc.Open();
				var cpu = pc.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.CPU);
				var sensors = cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature).ToArray();

				using (Aero aero = new Aero())
				{
					while (run)
					{
						cpu.Update();
						double wmiTemp = await aero.Cpu.GetTemperatureAsync();
						double monTemp = sensors.Max(s => s.Value ?? 0.0f);
						Console.Write($"{wmiTemp:F1}°C \t {monTemp:F1}°C");
						Console.CursorLeft = 0;
						await Task.Delay(50);
					}
				}

				pc.Close();
			}
		}
	}
}
