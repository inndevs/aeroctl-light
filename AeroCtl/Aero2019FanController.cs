using System;
using System.Management;
using System.Runtime.InteropServices;

namespace AeroCtl
{
	/// <summary>
	/// Implements the fan controller and thermal management of the notebook.
	/// </summary>
	public class Aero2019FanController : IFanController, IFanCurveController
	{
		#region Fields

		private readonly AeroWmi wmi;

		private const int minFanSpeed = 0;
		private const int maxFanSpeed = 229;

		#endregion

		#region Properties

		public int FanCurvePointCount => 15;

		/// <summary>
		/// Gets the current RPM of fan 1.
		/// </summary>
		public int Rpm1 => reverse(this.wmi.InvokeGet<ushort>("getRpm1"));

		/// <summary>
		/// Gets the current RPM of fan 2.
		/// </summary>
		public int Rpm2 => reverse(this.wmi.InvokeGet<ushort>("getRpm2"));

		public bool AutoFan
		{
			get => this.wmi.InvokeGet<byte>("GetAutoFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetAutoFanStatus", value ? (byte)1 : (byte)0);
		}

		public int FanAdjust
		{
			get => this.wmi.InvokeGet<byte>("GetFanAdjustStatus");
			set
			{
				if (value < minFanSpeed || value > maxFanSpeed)
					throw new ArgumentOutOfRangeException(nameof(value));

				this.wmi.InvokeSet<byte>("SetFanAdjustStatus", (byte)value);
			}
		}

		public bool MaxFan
		{
			get => this.wmi.InvokeGet<byte>("GetFanSpeed") != 0;
			set
			{
				try
				{
					this.wmi.InvokeSet<byte>("SetFanSpeed", value ? (byte) 1 : (byte) 0);
				}
				catch (ManagementException)
				{
					// Always throws an exception even though it has an effect.
				}
			}
		}

		public bool FixedFan
		{
			get => this.wmi.InvokeGet<ushort>("GetFixedFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetFixedFanStatus", value ? (byte)1 : (byte)0);
		}

		public int FixedFanSpeed
		{
			get => (byte)this.wmi.InvokeGet<ushort>("GetFixedFanSpeed");
			set
			{
				if (value < minFanSpeed || value > maxFanSpeed)
					throw new ArgumentOutOfRangeException(nameof(value));

				this.wmi.InvokeSet<byte>("SetFixedFanSpeed", (byte)value);
			}
		}

		public bool StepFan
		{
			get => this.wmi.InvokeGet<ushort>("GetStepFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetStepFanStatus", value ? (byte)1 : (byte)0);
		}

		public bool NvThermalTarget
		{
			get => this.wmi.InvokeGet<byte>("GetNvThermalTarget") != 0;
			set => this.wmi.InvokeSet<byte>("SetNvThermalTarget", value ? (byte)1 : (byte)0);
		}

		public bool NvPowerConfig
		{
			get => this.wmi.InvokeGet<byte>("GetNvPowerConfig") != 0;
			set => this.wmi.InvokeSet<byte>("SetNvPowerCfg", value ? (byte)1 : (byte)0);
		}

		#endregion

		#region Constructors

		public Aero2019FanController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#endregion

		#region Methods

		private static int relToAbs(double fanSpeed)
		{
			if (fanSpeed <= 0.0) return minFanSpeed;
			if (fanSpeed >= 1.0) return maxFanSpeed;
			return (int) (minFanSpeed + fanSpeed * (maxFanSpeed - minFanSpeed));
		}

		private static double absToRel(int fanSpeed)
		{
			return (double) (fanSpeed - minFanSpeed) / (maxFanSpeed - minFanSpeed);
		}

		public void SetQuiet()
		{
			this.FixedFan = false;
			this.MaxFan = false;
			this.StepFan = false;
			this.AutoFan = false;
			this.NvThermalTarget = true;
		}

		public void SetNormal()
		{
			this.FixedFan = false;
			this.MaxFan = false;
			this.StepFan = false;
			this.AutoFan = false;
			this.NvThermalTarget = false;
		}

		public void SetGaming()
		{
			this.FixedFan = false;
			this.MaxFan = false;
			this.StepFan = false;
			this.AutoFan = true;
			this.NvThermalTarget = false;
		}

		public void SetAuto(double fanAdjust = 0.25)
		{
			this.MaxFan = false;
			this.AutoFan = false;
			this.FixedFan = false;
			this.StepFan = true;
			this.NvThermalTarget = false;
			this.FanAdjust = (byte)relToAbs(fanAdjust);
		}

		public void SetFixed(double fanSpeed = 0.25)
		{
			this.MaxFan = false;
			this.AutoFan = false;
			this.StepFan = true;
			this.NvThermalTarget = false;
			this.FixedFan = true;
			this.FixedFanSpeed = (byte)relToAbs(fanSpeed);
		}

		/// <summary>
		/// Returns the fan point at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public FanPoint GetFanCurvePoint(int index)
		{
			if (index < 0 || index >= this.FanCurvePointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			ManagementBaseObject inParams = this.wmi.GetClass.GetMethodParameters("GetFanIndexValue");
			inParams["Index"] = (byte)index;
			ManagementBaseObject outParams = this.wmi.Get.InvokeMethod("GetFanIndexValue", inParams, null);

			return new FanPoint
			{
				Temperature = (byte)outParams["Temperture"], // sic
				FanSpeed = absToRel((byte)outParams["Value"]),
			};
		}

		/// <summary>
		/// Sets the fan point at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="point"></param>
		public void SetFanCurvePoint(int index, FanPoint point)
		{
			if (index < 0 || index >= this.FanCurvePointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("SetFanIndexValue");
			inParams["Index"] = (byte)index;
			inParams["Temperture"] = (byte)point.Temperature;
			inParams["Value"] = (byte)relToAbs(point.FanSpeed);
			this.wmi.Set.InvokeMethod("SetFanIndexValue", inParams, null);
		}

		/// <summary>
		/// Reverse a 16-bit integer byte order.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}

		#endregion
	}
}