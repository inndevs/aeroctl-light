using System;
using System.Threading.Tasks;
using AeroCtl;

namespace Rainbow
{
	public static class Program
	{
		private struct Rgb
		{
			public byte R;
			public byte G;
			public byte B;

			public Rgb(byte r, byte g, byte b)
			{
				this.R = r;
				this.G = g;
				this.B = b;
			}
		}

		private static Rgb HsvToRgb(double hue, double saturation, double value)
		{
			int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
			double f = hue / 60 - Math.Floor(hue / 60);

			value = value * 255;
			byte v = (byte)(value);
			byte p = (byte)(value * (1 - saturation));
			byte q = (byte)(value * (1 - f * saturation));
			byte t = (byte)(value * (1 - (1 - f) * saturation));

			switch (hi)
			{
				case 0:
					return new Rgb(v, t, p);
				case 1:
					return new Rgb(q, v, p);
				case 2:
					return new Rgb(p, v, t);
				case 3:
					return new Rgb(p, q, v);
				case 4:
					return new Rgb(t, p, v);
				default:
					return new Rgb(v, p, q);
			}
		}

		public static async Task Main(string[] args)
		{
			using (Aero aero = new Aero())
			{
				IRgbController rgb = aero.Keyboard.Rgb;

				byte[] image = new byte[512];

				double hStart = 0.0;
				for (;;)
				{
					double h = hStart;
					for (int i = 0; i < 128; ++i)
					{
						Rgb color = HsvToRgb(h, 1.0, 1.0);

						image[4 * i + 0] = (byte)i;
						image[4 * i + 1] = (byte)(color.R * 60 / 255);
						image[4 * i + 2] = color.G;
						image[4 * i + 3] = (byte)(color.B * 90 / 255);

						h += 1.0;
					}

					await rgb.SetEffectAsync(new RgbEffect {Type = RgbEffectType.Custom0, Brightness = 51});
					await rgb.SetImageAsync(0, image);
					await Task.Delay(1);
					hStart += 5.0;
				}
			}
		}
	}
}
