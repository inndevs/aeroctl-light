using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IFanController
	{
		ValueTask<(int fan1, int fan2)> GetRpmAsync();
		ValueTask<double> GetPwmAsync();
		FanCurve GetFanCurve();

		ValueTask SetQuietAsync();
		ValueTask SetNormalAsync();
		ValueTask SetGamingAsync();
		ValueTask SetFixedAsync(double fanSpeed = 0.25);
		ValueTask SetAutoAsync(double fanAdjust = 0.25);
		ValueTask SetCustomAsync();
	}

	public interface IFanControllerSync
	{
		void SetFixed(double fanSpeed = 0.25);
	}
}
