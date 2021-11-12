using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IFanController
	{
		ValueTask<(int fan1, int fan2)> GetRpmAsync();
		ValueTask<double> GetPwmAsync();
	}

	public interface IFanControllerSync
	{
		void SetFixed(double fanSpeed = 0.25);
	}
	
}
