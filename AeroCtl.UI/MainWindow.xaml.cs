using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int notificationTimeout = 3000;

		private readonly NotifyIcon trayIcon;
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

			this.InitializeComponent();

			this.trayIcon = new NotifyIcon
			{
				Icon = Properties.Resources.Main,
				Visible = true,
				Text = this.Title
			};

			this.trayIcon.DoubleClick += (s, e) =>
			{
				this.Show();
				this.WindowState = WindowState.Normal;
			};

			if (this.Aero.StartMinimized && !Debugger.IsAttached)
			{
				this.WindowState = WindowState.Minimized;
				this.Hide();
			}

			CancellationTokenSource cts = new CancellationTokenSource();
			Task updateTask = this.updateLoop(cts.Token);
			this.Closing += async (s, e) =>
			{
				cts.Cancel();
				
				try
				{
					await updateTask;
				}
				catch (OperationCanceledException)
				{

				}

				this.trayIcon.Dispose();
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

				case FnKey.ToggleFan:
					FanProfile fanProfile = this.Aero.FanProfileAlt;
					this.Aero.FanProfileAlt = this.Aero.FanProfile;
					this.Aero.FanProfile = fanProfile;
				
					this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, $"Fan profile switched to \"{fanProfile}\".", ToolTipIcon.Info);
					break;

				case FnKey.ToggleWifi:
					bool wifi = !this.aero.WifiEnabled;
					this.aero.WifiEnabled = wifi;

					this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, $"Wifi {(wifi ? "enabled" : "disabled")}.", ToolTipIcon.Info);
					break;

				case FnKey.ToggleScreen:
					this.aero.Screen.ToggleScreen();
					break;

				case FnKey.ToggleTouchpad:
					bool touchPad = !this.aero.Touchpad.Enabled;
					this.aero.Touchpad.Enabled = touchPad;

					this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, $"Touchpad {(touchPad ? "enabled" : "disabled")}.", ToolTipIcon.Info);
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
					if (first || this.Aero.FanProfileInvalid || this.WindowState != WindowState.Minimized)
					{
						await this.Aero.UpdateAsync(first);
						first = false;
					}

					await Task.Delay(750, token);
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
				this.aero.Fans.SetNormalAsync();
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
