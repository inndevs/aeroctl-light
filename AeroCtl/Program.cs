using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroCtl.UI;

namespace AeroCtl
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (AeroWmi wmi = new AeroWmi())
			using (Aero aero = new Aero(wmi))
			{
				Application.Run(new MainWindow(aero));
			}
		}
	}
}
