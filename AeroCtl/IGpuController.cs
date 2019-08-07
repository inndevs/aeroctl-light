using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// GPU controller interface.
	/// </summary>
	public interface IGpuController
	{
		/// <summary>
		/// Gets the current temperature of the GPU.
		/// </summary>
		/// <returns></returns>
		Task<double> GetTemperatureAsync();
	}
}
