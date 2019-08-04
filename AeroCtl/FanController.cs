using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace AeroCtl
{
	public struct FanPoint : IEquatable<FanPoint>
	{
		public int Temperature { get; set; }
		public int Speed { get; set; }

		public bool Equals(FanPoint other)
		{
			return Temperature == other.Temperature && Speed == other.Speed;
		}

		public override bool Equals(object obj)
		{
			return obj is FanPoint other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (this.Temperature * 397) ^ this.Speed;
			}
		}

		public static bool operator ==(FanPoint left, FanPoint right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FanPoint left, FanPoint right)
		{
			return !left.Equals(right);
		}
	}

	public class FanCurve : IList<FanPoint>
	{
		private readonly FanController controller;

		public FanPoint this[int index]
		{
			get => this.controller.GetFanPoint(index);
			set => this.controller.SetFanPoint(index, value);
		}

		public int Count => FanController.FanPointCount;

		public bool IsReadOnly => true;

		public FanCurve(FanController controller)
		{
			this.controller = controller;
		}

		public void Add(FanPoint item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(FanPoint item)
		{
			return ((IEnumerable<FanPoint>)this).Contains(item);
		}

		public void CopyTo(FanPoint[] array, int arrayIndex)
		{
			for (int i = 0; i < this.Count; ++i)
			{
				array[arrayIndex + i] = this[i];
			}
		}

		public IEnumerator<FanPoint> GetEnumerator()
		{
			for (int i = 0; i < this.Count; ++i)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public int IndexOf(FanPoint item)
		{
			for (int i = 0; i < this.Count; ++i)
			{
				if (Equals(this[i], item))
					return i;
			}
			return -1;
		}

		public void Insert(int index, FanPoint item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(FanPoint item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
	}

	public class FanController
	{
		#region Fields

		public const int FanPointCount = 15;

		private readonly AeroWmi wmi;

		#endregion

		#region Properties

		public int Rpm1 => reverse(this.wmi.InvokeGet<ushort>("getRpm1"));

		public int Rpm2 => reverse(this.wmi.InvokeGet<ushort>("getRpm2"));

		public bool AutoFan
		{
			get => this.wmi.InvokeGet<byte>("GetAutoFanStatus") != 0;
			set => this.wmi.InvokeSet<byte>("SetAutoFanStatus", value ? (byte)1 : (byte)0);
		}

		public byte FanAdjust
		{
			get => this.wmi.InvokeGet<byte>("GetFanAdjustStatus");
			set => this.wmi.InvokeSet<byte>("SetFanAdjustStatus", value);
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

		public byte FixedFanSpeed
		{
			get => (byte)this.wmi.InvokeGet<ushort>("GetFixedFanSpeed");
			set => this.wmi.InvokeSet<byte>("SetFixedFanSpeed", value);
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

		public FanController(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#endregion

		#region Methods

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

		public void SetCustomAuto()
		{
			this.FixedFan = false;
			this.MaxFan = false;
			this.StepFan = true;
			this.AutoFan = false;
			this.NvThermalTarget = false;
			this.FanAdjust = 50;
		}

		public void SetCustomFixed()
		{
			this.MaxFan = false;
			this.StepFan = true;
			this.AutoFan = false;
			this.NvThermalTarget = false;
			this.FixedFan = true;
			this.FixedFanSpeed = 50;
		}

		public FanPoint GetFanPoint(int index)
		{
			if (index < 0 || index >= FanPointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			ManagementBaseObject inParams = this.wmi.GetClass.GetMethodParameters("GetFanIndexValue");
			inParams["Index"] = (byte)index;
			ManagementBaseObject outParams = this.wmi.Get.InvokeMethod("GetFanIndexValue", inParams, null);

			return new FanPoint
			{
				Temperature = (byte)outParams["Temperture"], // sic
				Speed = (byte)outParams["Value"],
			};
		}

		public void SetFanPoint(int index, FanPoint point)
		{
			if (index < 0 || index >= FanPointCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			ManagementBaseObject inParams = this.wmi.SetClass.GetMethodParameters("SetFanIndexValue");
			inParams["Index"] = (byte)index;
			inParams["Temperture"] = (byte)point.Temperature;
			inParams["Value"] = (byte)point.Speed;
			this.wmi.Set.InvokeMethod("SetFanIndexValue", inParams, null);
		}

		private static ushort reverse(ushort val)
		{
			return (ushort)((val << 8) | (val >> 8));
		}

		#endregion
	}
}