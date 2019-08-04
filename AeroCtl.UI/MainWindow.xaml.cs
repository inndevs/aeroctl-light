using NativeWifi;
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
		
		public AeroController Aero { get; }

		public MainWindow()
		{
			this.wmi = new AeroWmi();
			this.aero = new Aero(this.wmi);
			this.aero.Keyboard.FnKeyPressed += onFnKeyPressed;

			this.Aero = new AeroController(this.aero);

			this.InitializeComponent();

			CancellationTokenSource cts = new CancellationTokenSource();
			this.Closing += async (s, e) =>
			{
				cts.Cancel();
				
				try
				{
					await this.updateTask;
				}
				catch (TaskCanceledException)
				{

				}
			};
			this.updateTask = this.updateLoop(cts.Token);
		}

		private void onFnKeyPressed(object sender, FnKeyEventArgs e)
		{
			switch(e.Key)
			{
				case FnKey.IncreaseBrightness:
					this.aero.Screen.Brightness = Math.Min(100, this.Aero.ScreenBrightness + 5);
					break;

				case FnKey.DecreaseBrightness:
					this.aero.Screen.Brightness = Math.Max(0, this.Aero.ScreenBrightness - 5);
					break;

				case FnKey.ToggleWifi:
					this.aero.WifiEnabled = !this.aero.WifiEnabled;
					break;

				case FnKey.ToggleScreen:
					this.aero.Screen.ToggleScreen();
					break;
			}
		}

		private async Task updateLoop(CancellationToken token)
		{
			await Task.Yield();
			try
			{
				for (; ; )
				{
					if (Mouse.Captured == null && this.WindowState != WindowState.Minimized)
						this.Aero.Update();

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

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			this.curveGrid.ItemsSource = new FanCurve(this.aero.Fans);
		}

		private void FanProfileQuiet_OnClick(object sender, RoutedEventArgs e)
		{
			this.aero.Fans.SetQuiet();
		}

		private void FanProfileNormal_OnClick(object sender, RoutedEventArgs e)
		{
			this.aero.Fans.SetNormal();
		}

		private void FanProfileGaming_OnClick(object sender, RoutedEventArgs e)
		{
			this.aero.Fans.SetGaming();
		}

		private void FanProfileAuto_OnClick(object sender, RoutedEventArgs e)
		{
			this.aero.Fans.SetCustomAuto();
		}

		private void FanProfileFixed_OnClick(object sender, RoutedEventArgs e)
		{
			this.aero.Fans.SetCustomFixed();
		}
	}
}
