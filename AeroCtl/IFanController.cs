using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IFanController
	{
		Task<(int fan1, int fan2)> GetRpmAsync();
		Task SetQuietAsync();
		Task SetNormalAsync();
		Task SetGamingAsync();
		Task SetFixedAsync(double fanSpeed = 0.25);
		Task SetAutoAsync(double fanAdjust = 0.25);
		Task SetCustomAsync();
		FanCurve GetFanCurve();
	}
}
