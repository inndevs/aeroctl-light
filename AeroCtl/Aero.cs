using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Implements the AERO interfaces.
	/// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private readonly AeroWmi wmi;

		#endregion

		#region Properties

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyHandler Keys { get; }

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public FanController Fans { get; }

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.wmi = wmi;

			this.Keys = new KeyHandler();
			this.Fans = new FanController(wmi);
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			this.wmi?.Dispose();
			this.Keys?.Dispose();
		}

		#endregion
	}
}
