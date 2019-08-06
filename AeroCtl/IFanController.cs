using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IFanController
	{
		int Rpm1 { get; }
		int Rpm2 { get; }

		void SetQuiet();
		void SetNormal();
		void SetGaming();
		void SetFixed(double fanSpeed = 0.25);
		void SetAuto(double fanAdjust = 0.25);
		void SetCustom();
		FanCurve GetFanCurve();
	}
}
