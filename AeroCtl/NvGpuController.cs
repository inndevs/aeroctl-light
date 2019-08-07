using System;
using System.Threading.Tasks;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;

namespace AeroCtl
{
	public class NvGpuController : IGpuController
	{
		public PhysicalGPU Gpu { get; }

		public NvGpuController(PhysicalGPU gpu)
		{
			this.Gpu = gpu;
		}

		public Task<double> GetTemperatureAsync()
		{
			double max = 0.0;

			if (this.Gpu != null)
			{
				try
				{
					foreach (var sensor in this.Gpu.ThermalInformation.ThermalSensors)
					{
						max = Math.Max(sensor.CurrentTemperature, max);
					}
				}
				catch (NVIDIAApiException ex) when (ex.Status == (Status)(-220)) /* gpu not powered */
				{

				}
			}

			return Task.FromResult(max);

		}
	}
}