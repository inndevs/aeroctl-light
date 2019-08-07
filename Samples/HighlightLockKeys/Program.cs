using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AeroCtl;

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
			cts.Token.Register(() => f.Invoke(new Action(f.Close)));

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

		private static async Task effect(CancellationToken cancellationToken)
		{
			for (;;)
			{
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

					for (int i = 0; i < 120; ++i)
					{
						setColor(i, baseColor);
					}

					State currentState = (State)(-1);
					int page = 0;

					for (;;)
					{
						await Task.Delay(200, cancellationToken);

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

							setColor(capsLockKey, (currentState & State.Caps) != 0 ? highlightColor : baseColor);

							setColor(numLockKey, (currentState & State.Scroll) != 0 ? highlightColor : baseColor);

							foreach (int k in numPadKeys)
							{
								setColor(k, (currentState & State.Num) != 0 ? highlightColor : baseColor);
							}

							cancellationToken.ThrowIfCancellationRequested();

							try
							{
								int brightness = (await ctl.Rgb.GetEffectAsync()).Brightness;

								await ctl.Rgb.SetImageAsync(page, image);
								await ctl.Rgb.SetEffectAsync(new RgbEffect
								{
									Type = RgbEffectType.Custom0,
									Brightness = brightness,
								});
							}
							catch (IOException)
							{
								break;
							}

							page = (page + 1) & 1;
						}
					}
				}
			}
		}
	}
}