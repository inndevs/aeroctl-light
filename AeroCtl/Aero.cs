using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	public class Aero
	{
		#region Fields

		private readonly AeroWmi wmi;

		#endregion

		#region Properties

		public FanController Fans { get; }

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.wmi = wmi;
		}

		#endregion

		#region Methods

		#endregion
	}
}
