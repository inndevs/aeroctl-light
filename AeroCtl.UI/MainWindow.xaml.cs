using System;
using System.ComponentModel;
using System.Windows;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool suppressShutdownOnClose;

		public AeroController Controller { get; }

		public MainWindow(AeroController controller)
		{
			this.Controller = controller;
			this.InitializeComponent();
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (this.WindowState == WindowState.Minimized)
			{
				// Minimizing should close the window, but not exist the app.
				this.suppressShutdownOnClose = true;
				this.Close();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (this.suppressShutdownOnClose) return;

			Application.Current.Shutdown();
		}

	}

}
