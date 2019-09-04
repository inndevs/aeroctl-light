using System;
using System.Linq;
using OpenHardwareMonitor.Hardware;

namespace AeroCtl.UI
{
	/// <summary>
	/// Implements CPU and GPU monitoring.
	/// </summary>
	public class HwMonitor : IDisposable
	{
		private Computer computer;
		private IHardware cpu;
		private IHardware gpu;

		public double CpuTemperature => this.cpu?.Sensors.FirstOrDefault(s => string.Equals(s.Name, "cpu package", StringComparison.OrdinalIgnoreCase))?.Value ?? 0.0;

		public double GpuTemperature => this.gpu?.Sensors.Where(s => s.SensorType == SensorType.Temperature).Max(s => s.Value ?? 0) ?? 0.0;
		
		public HwMonitor()
		{
			this.computer = new Computer
			{
				CPUEnabled = true,
				GPUEnabled = true
			};

			this.computer.Open();

			this.cpu = this.computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.CPU);
			this.gpu = this.computer.Hardware.FirstOrDefault(hw => hw.HardwareType == HardwareType.GpuNvidia);
		}

		public void Update()
		{
			this.cpu?.Update();
			this.gpu?.Update();
		}

		public void Dispose()
		{
			this.computer?.Close();
			this.computer = null;
			this.cpu = null;
			this.gpu = null;
		}
	}
}
