using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// CPU controller interface.
	/// </summary>
	public interface ICpuController
	{
		ValueTask<double> GetTemperatureAsync();
	}
}