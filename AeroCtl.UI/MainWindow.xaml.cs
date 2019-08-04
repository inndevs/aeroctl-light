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

		public AeroController Aero { get; }

		public MainWindow()
		{
			this.wmi = new AeroWmi();
			this.aero = new Aero(this.wmi);
			this.aero.Keyboard.FnKeyPressed += onFnKeyPressed;

			this.Aero = new AeroController(this.aero);

			this.InitializeComponent();

			CancellationTokenSource cts = new CancellationTokenSource();
			Task updateTask = this.updateLoop(cts.Token);
			this.Closing += async (s, e) =>
			{
				cts.Cancel();
				
				try
				{
					await updateTask;
				}
				catch (TaskCanceledException)
				{

				}
			};
		}

		private void onFnKeyPressed(object sender, FnKeyEventArgs e)
		{
			switch(e.Key)
			{
				case FnKey.IncreaseBrightness:
					this.aero.Screen.Brightness = Math.Min(100, this.aero.Screen.Brightness + 5);
					break;

				case FnKey.DecreaseBrightness:
					this.aero.Screen.Brightness = Math.Max(0, this.aero.Screen.Brightness - 5);
					break;

				case FnKey.ToggleWifi:
					this.aero.WifiEnabled = !this.aero.WifiEnabled;
					break;

				case FnKey.ToggleScreen:
					this.aero.Screen.ToggleScreen();
					break;
			}
		}

		private async Task kbdTest(CancellationToken token)
		{
			//LightColor color = 0;
			await this.aero.Keyboard.Rgb.SetEffectAsync(LightEffect.Static, 1, 50, LightColor.Orange, 0);

			byte[] temp = new byte[512];

			for (int i = 0; i < temp.Length; i += 4)
			{
				temp[i + 0] = 0;
				temp[i + 1] = 0;
				temp[i + 2] = 200;
				temp[i + 3] = 255;
			}

			await this.aero.Keyboard.Rgb.SetEffectAsync(LightEffect.Custom1, 5, 50, LightColor.Green, 0);
			await this.aero.Keyboard.Rgb.SetImageAsync(0, temp);
		}

		private async Task updateLoop(CancellationToken token)
		{
			bool first = true;

			Task test = Task.Run(() => this.kbdTest(token));

			await Task.Yield();
			try
			{
				for (;;)
				{
					if (Mouse.Captured == null && this.WindowState != WindowState.Minimized)
					{
						await this.Aero.UpdateAsync(first);
						first = false;
					}

					await Task.Delay(500, token);
				}
			}
			finally
			{
				await test;
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
