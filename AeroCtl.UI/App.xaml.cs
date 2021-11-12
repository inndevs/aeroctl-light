using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace AeroCtl.UI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private const int notificationTimeout = 3000;

		private readonly string title;
		private readonly CancellationTokenSource cancellationTokenSource;
		private NotifyIcon trayIcon;
		private Aero aero;
		private AeroController controller;
		private Task updateTask;

		private readonly object windowLock;
		private MainWindow window;

		public App()
		{
			this.title = typeof(App).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? typeof(App).Assembly.GetName().Name;
			this.cancellationTokenSource = new CancellationTokenSource();
			this.windowLock = new object();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Create aero and controller.
			this.aero = new Aero();
			this.controller = new AeroController(this.aero);
			this.controller.Load();

			// Create background update task.
			this.updateTask = this.Dispatcher.InvokeAsync(() => this.updateLoop(this.cancellationTokenSource.Token), DispatcherPriority.Background).Task;

			// Get app icon.
			Icon ico;
			using (Stream stream = typeof(App).Assembly.GetManifestResourceStream("AeroCtl.UI.Main.ico"))
			{
				ico = new Icon(stream);
			}

			// Create tray icon.
			this.trayIcon = new NotifyIcon
			{
				Icon = ico,
				Text = this.title,
				Visible = true,
			};

			this.trayIcon.DoubleClick += (s, e2) => { this.showWindow(); };

			// bind events to reinit settings after hibernate etc.
			SystemEvents.SessionSwitch += this.onSessionSwitch;
			SystemEvents.PowerModeChanged += this.onPowerModeChanged;

			if (!this.controller.StartMinimized || Debugger.IsAttached)
			{
				// Show window if 'start minimized' isn't active.
				this.showWindow();
			}
		}



		private void showWindow()
		{
			lock (this.windowLock)
			{
				if (this.window == null)
				{
					// Create window if it doesn't exist.
					this.window = new MainWindow(this.controller);

					// Register close handler to set window back to null.
					this.window.Closed += (s, e) =>
					{
						lock (this.windowLock)
						{
							this.window = null;
						}
					};
				}

				// Show window and restore if minimized.
				this.window.Show();
				this.window.WindowState = WindowState.Normal;
				this.window.Focus();
			}
		}

		private async Task shutdownAsync()
		{
			// Cancel.
			this.cancellationTokenSource.Cancel();

			// Wait for aero update task to stop.
			try
			{
				await this.updateTask;
			}
			catch (OperationCanceledException)
			{ }
			finally
			{
				lock (this.windowLock)
					this.window?.Close();

				// Remove tray icon.
				this.trayIcon.Dispose();

				// Unregister events (probably not necessary but whatever).
				SystemEvents.SessionSwitch -= this.onSessionSwitch;
				SystemEvents.PowerModeChanged -= this.onPowerModeChanged;

				// Close aero.
				this.aero.Dispose();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			SynchronizationContext.SetSynchronizationContext(null);
			this.shutdownAsync().Wait();
			base.OnExit(e);
		}

		private async Task updateLoop(CancellationToken token)
		{
			bool first = true;

			await Task.Yield();
			try
			{
				for (; ; )
				{
					UpdateMode mode = UpdateMode.Light;

					if (first)
					{
						mode = UpdateMode.Full;
						first = false;
					}
					else if (this.window != null && this.window.WindowState != WindowState.Minimized)
					{
						mode = UpdateMode.Normal;
					}

					await this.controller.UpdateAsync(mode);
					await Task.Delay(750, token);
				}
			}
			catch (OperationCanceledException)
			{
				return;
			}
			catch (Exception ex)
			{
				StringBuilder str = new();
				str.AppendLine($"Voll aggressiver Fehler.");
				str.AppendLine();
				str.AppendLine("Exception details:");
				str.Append(ex.ToString());
				if (System.Windows.MessageBox.Show(str.ToString(), "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
				{
					Process.Start(new ProcessStartInfo("https://gitlab.com/wtwrp/aeroctl/issues")
					{
						UseShellExecute = true
					});
				}

				throw;
			}
			finally
			{
				this.Shutdown();
			}
		}

		private void onSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			// do stuff after someone logs in or out (or when the laptop comes back from sleep)
		}

		private void onPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			// do stuff after hibernation resume.
		}
		
	}
}
