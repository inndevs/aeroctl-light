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

				using (AeroWmi wmi = new AeroWmi())
				using (Aero aero = new Aero(wmi))
				{
					while(run)
					{
						cpu.Update();
						double wmiTemp = await aero.Cpu.GetTemperatureAsync();
						double monTemp = sensors.Max(s => s.Value ?? 0.0f);
						Console.WriteLine($"{wmiTemp:F1}°C | {monTemp:F1}°C");
						await Task.Delay(250);
					}
				}

				pc.Close();
			}

		}
	}
}
