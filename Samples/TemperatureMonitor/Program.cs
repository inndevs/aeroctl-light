using System;
using System.Linq;
using System.Threading.Tasks;
using AeroCtl;
using AeroCtl.UI;
using LibreHardwareMonitor.Hardware;

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

			var computer = new Computer
			{
				IsCpuEnabled = true,
				IsGpuEnabled = true
			};

			computer.Open();

			var cpu = computer.Hardware.First(hw => hw.HardwareType == HardwareType.Cpu);
			cpu.Update();

			foreach (var sens in cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature))
			{
				Console.WriteLine($"{sens.Name} = {sens.Value}");
			}

			computer.Close();
			
			using (HwMonitor hw = new HwMonitor())
			using (Aero aero = new Aero())
			{
				//Random rng = new Random();
				//IDirectFanSpeedController fans = (IDirectFanSpeedController)aero.Fans;

				while (run)
				{
					hw.Update();
					double wmiCpuTemp = await aero.Cpu.GetTemperatureAsync();
					double monCpuTemp = hw.CpuTemperature;
					double monGpuTemp = hw.GpuTemperature;
					Console.Write($"CPU {wmiCpuTemp:F1}°C \t {monCpuTemp:F1}°C \t GPU {monGpuTemp:F1}°C   ");
					Console.CursorLeft = 0;
					//fans.SetFixed(0.0 + rng.NextDouble() * 0.3);
					await Task.Delay(500);
				}
			}
		}
	}
}
