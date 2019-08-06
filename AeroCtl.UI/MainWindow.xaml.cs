using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
			this.Aero.Load();

			System.Windows.Forms.NotifyIcon trayIcon = new System.Windows.Forms.NotifyIcon
			{
				Icon = Properties.Resources.Main,
				Visible = true,
				Text = "AERO Controls"
			};

			trayIcon.DoubleClick += (s, e) =>
			{
				this.Show();
				this.WindowState = WindowState.Normal;
			};

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

				trayIcon.Dispose();
			};
		}

		private void onFnKeyPressed(object sender, FnKeyEventArgs e)
		{
			switch(e.Key)
			{
				case FnKey.IncreaseBrightness:
					this.aero.Screen.Brightness = Math.Min(100, this.aero.Screen.Brightness + 10);
					break;

				case FnKey.DecreaseBrightness:
					this.aero.Screen.Brightness = Math.Max(0, this.aero.Screen.Brightness - 10);
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
			bool first = true;

			await Task.Yield();
			try
			{
				for (;;)
				{
					if (this.WindowState != WindowState.Minimized)
					{
						await this.Aero.UpdateAsync(first);
						first = false;
					}

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

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (this.WindowState == WindowState.Minimized)
				this.Hide();
		}
		
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (this.Aero.FanProfile == FanProfile.Software)
			{
				this.aero.Fans.SetNormalAsync().Wait();
			}
		}

		private void onEditHwCurveClicked(object sender, RoutedEventArgs e)
		{
			FanCurve curve = this.aero.Fans.GetFanCurve();
			FanPoint[] clone = curve.ToArray();
			FanCurveEditor editor = new FanCurveEditor(clone, FanCurveKind.Step);
			editor.CurveApplied += (s, e2) =>
			{
				for (int i = 0; i < curve.Count; ++i)
				{
					curve[i] = clone[i];
				}
			};
			editor.ShowDialog();
		}

		private void onEditSwCurveClicked(object sender, RoutedEventArgs e)
		{
			List<FanPoint> curve = new List<FanPoint>(this.Aero.SoftwareFanCurve);
			FanCurveEditor editor = new FanCurveEditor(curve, FanCurveKind.Linear);
			editor.CurveApplied += (s, e2) =>
			{
				this.Aero.SoftwareFanCurve = curve.ToArray();
			};
			editor.ShowDialog();
		}
	}
}
