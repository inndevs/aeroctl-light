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
			Scroll = 4,
			LidClosed = 8,
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

		private static async Task<bool> apply(IRgbController rgb, State state)
		{
			if ((state & State.LidClosed) != 0)
			{
				// Turn off when lid is closed.
				try
				{
					int brightness = (await rgb.GetEffectAsync()).Brightness;
					await rgb.SetEffectAsync(new RgbEffect
					{
						Type = RgbEffectType.Static,
						Color = RgbEffectColor.Black,
						Brightness = brightness,
					});

					return true;
				}
				catch (Win32Exception)
				{
					return false;
				}
				catch (IOException)
				{
					return false;
				}
			}

			byte[] image = new byte[512];

			void setColor(int key, Color color)
			{
				image[4 * key + 0] = (byte)key;
				image[4 * key + 1] = color.R;
				image[4 * key + 2] = color.G;
				image[4 * key + 3] = color.B;
			}

			// Fill with base color.
			for (int i = 0; i < 128; ++i)
				setColor(i, baseColor);

			// Apply changes to certain keys / areas.
			setColor(capsLockKey, (state & State.Caps) != 0 ? highlightColor : baseColor);
			setColor(numLockKey, (state & State.Scroll) != 0 ? highlightColor : baseColor);
			foreach (int k in numPadKeys)
				setColor(k, (state & State.Num) == 0 ? highlightColor : baseColor);

			// Try to apply the effect.
			// Under certain conditions this seems to fail because the USB device gets disconnected (e.g. waking from sleep).
			// In that case it will exit the inner loop in order to try again.
			try
			{
				// Read current brightness from controller. This can be changed independently from this app by the user through
				// the keyboard brightness shortcut (Fn + Space).
				int brightness = (await rgb.GetEffectAsync()).Brightness;

				// Set new image.
				await rgb.SetImageAsync(0, image);
				await rgb.SetEffectAsync(new RgbEffect
				{
					Type = RgbEffectType.Custom0,
					Brightness = brightness,
				});

				return true;
			}
			catch (Win32Exception)
			{
				return false;
			}
			catch (IOException)
			{
				return false;
			}
		}

		/// <summary>
		/// Manages the keyboard effect.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private static async Task effect(CancellationToken cancellationToken)
		{
			int unsuccessfulAttempts = 0;
			
			for (;;)
			{
				cancellationToken.ThrowIfCancellationRequested();
				using (AeroWmi wmi = new AeroWmi())
				using (Aero aero = new Aero(wmi))
				{
					State currentState = 0;
					bool invalid = true;

					SystemEvents.SessionSwitch += (s, e) =>
					{
						// Re-apply the effect. It seems that it gets corrupted sometimes for unknown reasons.
						if (e.Reason == SessionSwitchReason.SessionUnlock ||
						    e.Reason == SessionSwitchReason.SessionLock)
							invalid = true;
					};

					TimeSpan lidCheckInterval = TimeSpan.FromSeconds(2);
					DateTime nextLidCheck = DateTime.Now;

					for (;;)
					{
						await Task.Delay(200, cancellationToken);

						// Read current keyboard state.
						State newState = currentState;

						if (Control.IsKeyLocked(Keys.CapsLock))
							newState |= State.Caps;
						else
							newState &= ~State.Caps;

						if (Control.IsKeyLocked(Keys.NumLock))
							newState |= State.Num;
						else
							newState &= ~State.Num;

						if (Control.IsKeyLocked(Keys.Scroll))
							newState |= State.Scroll;
						else
							newState &= ~State.Scroll;

						if (DateTime.Now >= nextLidCheck)
						{
							nextLidCheck = DateTime.Now + lidCheckInterval;

							if (await aero.Display.GetLidStatus() == LidStatus.Closed)
								newState |= State.LidClosed;
							else
								newState &= ~State.LidClosed;
						}

						if (currentState != newState || invalid)
						{
							cancellationToken.ThrowIfCancellationRequested();
							
							currentState = newState;
							invalid = false;

							bool success = await apply(aero.Keyboard.Rgb, currentState);
							if (success)
							{
								unsuccessfulAttempts = 0;
							}
							else
							{
								++unsuccessfulAttempts;
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