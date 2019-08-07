using NvAPIWrapper.GPU;

namespace AeroCtl
{
	/// <summary>
	/// Controller for the newer Aero models that expose additional GPU settings.
	/// </summary>
	public class Aero2019GpuController : NvGpuController
	{
		private readonly AeroWmi wmi;

		public bool PowerConfig
		{
			get => this.wmi.InvokeGet<byte>("GetNvPowerConfig") != 0;
			set => this.wmi.InvokeSet<byte>("SetNvPowerConfig", value ? (byte)1 : (byte)0);
		}

		public bool BoostEnabled
		{
			get => this.wmi.InvokeGet<byte>("GetAIBoostStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetAIBoostStatus", value ? (byte)1 : (byte)0);
		}

		public Aero2019GpuController(PhysicalGPU gpu, AeroWmi wmi) 
			: base(gpu)
		{
			this.wmi = wmi;
		}
	}
}