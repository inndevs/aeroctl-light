using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AeroCtl.UI.SoftwareFan;
using Microsoft.Win32;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int notificationTimeout = 3000;

		private bool shutdownComplete;

		private readonly TaskFactory taskFactory;
		private readonly NotifyIcon trayIcon;
		private readonly Aero aero;

		public AeroController Aero { get; }

		public MainWindow()
		{
			TaskScheduler taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			this.taskFactory = new TaskFactory(taskScheduler);

			this.aero = new Aero();
			this.aero.Keyboard.FnKeyPressed += (s, e) =>
			{
				this.taskFactory.StartNew(() => this.handleFnKey(e));
			};
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

			// To re-apply fan profile after wake up:
			SystemEvents.SessionSwitch += this.onSessionSwitch;
			SystemEvents.PowerModeChanged += this.onPowerModeChanged;
		}

		private async Task shutdown()
		{
			SystemEvents.SessionSwitch -= this.onSessionSwitch;
			SystemEvents.PowerModeChanged -= this.onPowerModeChanged;

			if (this.Aero.FanProfile == FanProfile.Software)
			{
				await this.aero.Fans.SetNormalAsync();
			}

			await this.Aero.DisposeAsync();
			this.aero.Dispose();

			this.trayIcon.Dispose();

			this.shutdownComplete = true;
			this.Close();
		}

		private void onSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			if (e.Reason == SessionSwitchReason.SessionUnlock || 
			    e.Reason == SessionSwitchReason.SessionLock)
				this.Aero.FanProfileInvalid = true;
		}

		private void onPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Resume)
				this.Aero.FanProfileInvalid = true;
		}

		private async Task handleFnKey(FnKeyEventArgs e)
		{
			switch(e.Key)
			{
				case FnKey.IncreaseBrightness:
					this.aero.Display.Brightness = Math.Min(100, this.aero.Display.Brightness + 10);
					break;

				case FnKey.DecreaseBrightness:
					this.aero.Display.Brightness = Math.Max(0, this.aero.Display.Brightness - 10);
					break;

				case FnKey.ToggleFan:
					FanProfile fanProfile = this.Aero.FanProfileAlt;
					this.Aero.FanProfileAlt = this.Aero.FanProfile;
					this.Aero.FanProfile = fanProfile;
				
					this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, $"Fan profile switched to \"{fanProfile}\".", ToolTipIcon.Info);
					break;

				case FnKey.ToggleWifi:
					bool? currentState = await this.aero.GetWifiEnabledAsync();
					if (currentState == null)
					{
						this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, "Could not determine wifi state.", ToolTipIcon.Warning);
					}
					else
					{
						bool newState = !currentState.Value;
						await this.aero.SetWifiEnabledAsync(newState);
						this.trayIcon.ShowBalloonTip(notificationTimeout, this.Title, $"Wifi {(newState ? "enabled" : "disabled")}.", ToolTipIcon.Info);
					}

					break;

				case FnKey.ToggleScreen:
					await this.aero.Display.ToggleScreenAsync();
					break;

				case FnKey.ToggleTouchpad:
					bool touchPad = !await this.aero.Touchpad.GetEnabledAsync();
					await this.aero.Touchpad.SetEnabledAsync(touchPad);

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
			if (!this.shutdownComplete)
			{
				this.taskFactory.StartNew(this.shutdown);
				e.Cancel = true;
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
			FanConfig cfg = new FanConfig(this.Aero.SoftwareFanConfig);
			List<FanPoint> curve = new List<FanPoint>(cfg.Curve);
			FanCurveEditor editor = new FanCurveEditor(curve, FanCurveKind.Linear);
			editor.CurveApplied += (s, e2) =>
			{
				cfg.Curve = curve.ToImmutableArray();
				this.Aero.SoftwareFanConfig = cfg;
			};
			editor.ShowDialog();
		}

		private void onEditSwconfigClicked(object sender, RoutedEventArgs e)
		{
			FanConfig cfg = new FanConfig(this.Aero.SoftwareFanConfig);
			FanConfigEditor editor = new FanConfigEditor(cfg);

			if (editor.ShowDialog() == true)
			{
				this.Aero.SoftwareFanConfig = cfg;
			}
		}
	}
}
