using System.Threading;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	public interface ISoftwareFanProvider
	{
		Task<double> GetTemperatureAsync(CancellationToken cancellationToken);

		Task SetSpeedAsync(double speed, CancellationToken cancellationToken);
	}
}