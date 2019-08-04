using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly Aero aero;
		private readonly AeroWmi wmi;

		private Task updateTask;	
		
		public AeroState AeroState { get; }

		public MainWindow()
		{
			this.wmi = new AeroWmi();
			this.aero = new Aero(this.wmi);
			this.AeroState = new AeroState(this.aero);

			this.InitializeComponent();
			
			this.DataContext = this.AeroState;

			CancellationTokenSource cts = new CancellationTokenSource();
			this.Closed += (s, e) => cts.Cancel();
			this.updateTask = this.updateLoop(cts.Token);
		}

		private async Task updateLoop(CancellationToken token)
		{
			await Task.Yield();
			try
			{
				for (; ; )
				{
					this.AeroState.Update();
					await Task.Delay(500, token);
				}
			}
			finally
			{
				this.aero.Dispose();
				this.wmi.Dispose();

				this.Close();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			this.updateTask.Wait();
			base.OnClosed(e);
		}
	}
}
