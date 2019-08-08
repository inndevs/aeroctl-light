using System.Threading;
using System.Threading.Tasks;

namespace AeroCtl.UI
{
	public interface ISoftwareFanProvider
	{
		ValueTask<double> GetTemperatureAsync(CancellationToken cancellationToken);
		ValueTask SetSpeedAsync(double speed, CancellationToken cancellationToken);
	}
}