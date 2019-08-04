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

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi { get; }

		public string BaseBoard => this.Wmi.BaseBoard;

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyHandler Keys { get; }

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public FanController Fans { get; }

		/// <summary>
		/// Gets the screen controller.
		/// </summary>
		public ScreenController Screen { get; }

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.Wmi = wmi;
			this.Keys = new KeyHandler();
			this.Fans = new FanController(wmi);
			this.Screen = new ScreenController(wmi);
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			this.Wmi?.Dispose();
			this.Keys?.Dispose();
		}

		#endregion
	}
}
