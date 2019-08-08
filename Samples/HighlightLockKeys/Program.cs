using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroCtl;
using Microsoft.Win32;

namespace HighlightLockKeys
{
	public static class Program
	{
		[STAThread]
		public static async Task Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Form f = new Form();

			f.Shown += (s, e) => { f.Hide(); };

			NotifyIcon notifyIcon = new NotifyIcon
			{
				Text = "AERO RGB Keyboard Thingy",
				Icon = new Icon(typeof(Program).Assembly.GetManifestResourceStream("HighlightLockKeys.Main.ico")),
				Visible = true
			};

			notifyIcon.DoubleClick += (s, e) => f.Close();

			CancellationTokenSource cts = new CancellationTokenSource();
			cts.Token.Register(() =>
			{
				if (!f.Created)
					return;

				f.Invoke(new Action(f.Close));
			});

			Task task = Task.Run(async () =>
			{
				try
				{
					await effect(cts.Token);
				}
				finally
				{
					cts.Cancel();
				}
			}, cts.Token);

			Application.Run(f);

			cts.Cancel();
			notifyIcon.Dispose();

			try
			{
				await task;
			}
			catch (OperationCanceledException)
			{

			}
		}

		[Flags]
		private enum State
		{
			Caps = 1,
			Num = 2,
			Scroll = 4
		}

		private static readonly Color baseColor = Color.FromArgb(0, 100, 40);
		private static readonly Color highlightColor = Color.FromArgb(0, 255, 30);

		private const int capsLockKey = 8;
		private const int numLockKey = 100;

		private static readonly int[] numPadKeys =
		{
			97, 98, 99,
			102, 103, 104, 105,
			108, 109, 110, 111,
		};

		/// <summary>
		/// Manages the keyboard effect.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private static async Task effect(CancellationToken cancellationToken)
		{
			for (;;)
			{
				int unsuccessfulAttempts = 0;

				cancellationToken.ThrowIfCancellationRequested();
				using (KeyboardController ctl = new KeyboardController())
				{
					byte[] image = new byte[512];

					void setColor(int key, Color color)
					{
						image[4 * key + 1] = color.R;
						image[4 * key + 2] = color.G;
						image[4 * key + 3] = color.B;
					}

					// Prepare image.
					for (int i = 0; i < 120; ++i)
					{
						setColor(i, baseColor);
					}

					State currentState = (State)(-1);
					int page = 0;

					SystemEvents.SessionSwitch += (s, e) =>
					{
						if (e.Reason == SessionSwitchReason.SessionUnlock ||
						    e.Reason == SessionSwitchReason.SessionLock)
							currentState = (State)(-1);
					};

					for (;;)
					{
						await Task.Delay(200, cancellationToken);

						// Read current keyboard state.
						State newState = 0;
					
						if (Control.IsKeyLocked(Keys.CapsLock))
							newState |= State.Caps;
					
						if (Control.IsKeyLocked(Keys.NumLock))
							newState |= State.Num;

						if (Control.IsKeyLocked(Keys.Scroll))
							newState |= State.Scroll;

						if (currentState != newState)
						{
							currentState = newState;

							// Apply changes to certain keys / areas.

							setColor(capsLockKey, (currentState & State.Caps) != 0 ? highlightColor : baseColor);
							setColor(numLockKey, (currentState & State.Scroll) != 0 ? highlightColor : baseColor);
							foreach (int k in numPadKeys)
								setColor(k, (currentState & State.Num) == 0 ? highlightColor : baseColor);
							
							cancellationToken.ThrowIfCancellationRequested();

							++unsuccessfulAttempts;
							
							// Try to apply the effect.
							// Under certain conditions this seems to fail because the USB device gets disconnected (e.g. waking from sleep).
							// In that case it will exit the inner loop in order to try again.
							try
							{
								// Read current brightness from controller. This can be changed independently from this app by the user through
								// the keyboard brightness shortcut (Fn + Space).
								int brightness = (await ctl.Rgb.GetEffectAsync()).Brightness;

								// First, set the new image to a page not currently shown.
								await ctl.Rgb.SetImageAsync(page, image);

								// Switch to that page.
								await ctl.Rgb.SetEffectAsync(new RgbEffect
								{
									Type = RgbEffectType.Custom0,
									Brightness = brightness,
								});

								// Swap pages.
								page = (page + 1) & 1;
								unsuccessfulAttempts = 0;
							}
							catch (Win32Exception)
							{
								break;
							}
							catch (IOException)
							{
								break;
							}
						}
					}
				}

				// If the controller keeps failing there must be something else wrong with it.
				// Exit after 3 attempts.
				if (unsuccessfulAttempts > 3)
					break;
			}
		}
	}
}