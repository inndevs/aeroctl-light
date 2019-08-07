using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AeroCtl.Native;

namespace AeroCtl
{
	/// <summary>
	/// Implements logic to talk with the USB HID device that controls keyboard LEDs.
	/// The chip appears to be called "ITE8291" according to its firmware blob.
	/// </summary>
	public class ITE8291RgbController : IRgbController
	{
		private readonly HidDevice device;
		private const int defaultWait = 1;

		public ITE8291RgbController(HidDevice device)
		{
			this.device = device;
		}

		public async Task<Version> GetFirmwareVersionAsync()
		{
			Packet res = await this.ExecAsync(new Packet {B1 = 128});

			if (res.B3 >= 10)
				return new Version(res.B2, res.B3 / 10, res.B3 % 10);
			return new Version(res.B2, res.B3);
		}

		public async Task SetEffectAsync(RgbEffect effect)
		{
			this.Set(new Packet
			{
				B1 = 8,
				B3 = (byte)effect.Type,
				B4 = (byte)effect.Speed,
				B5 = (byte)effect.Brightness,
				B6 = (byte)effect.Color,
				B7 = (byte)effect.Direction
			});
			await Task.Delay(defaultWait);
		}

		public async Task<RgbEffect> GetEffectAsync()
		{
			Packet res = await this.ExecAsync(new Packet {B1 = 136});
			return new RgbEffect
			{
				Type = (RgbEffectType)res.B3,
				Speed = res.B4,
				Brightness = res.B5,
				Color = (RgbEffectColor)res.B6,
				Direction = res.B7
			};
		}

		public async Task SetImageAsync(int index, ReadOnlyMemory<byte> image)
		{
			this.Set(new Packet {B1 = 18, B3 = (byte)index, B4 = 8});
			await Task.Delay(defaultWait);

			byte[] temp = new byte[65];

			for (int i = 0; i < 8; ++i)
			{
				image.Span.Slice(i * 64, 64).CopyTo(new Span<byte>(temp, 1, 64));
				this.device.Stream.Write(temp, 0, temp.Length);
				this.device.Stream.Flush();
				//await Task.Delay(defaultWait);
			}
		}

		public async Task<Packet> ExecAsync(Packet p, int delay = defaultWait)
		{
			this.Set(p);
			await Task.Delay(delay);
			return this.Get();
		}

		public void Set(Packet p)
		{
			Span<byte> buf = stackalloc byte[9];
			buf[0] = p.B0;
			buf[1] = p.B1;
			buf[2] = p.B2;
			buf[3] = p.B3;
			buf[4] = p.B4;
			buf[5] = p.B5;
			buf[6] = p.B6;
			buf[7] = p.B7;
			buf[8] = (byte) (0xFF - (p.B0 + p.B1 + p.B2 + p.B3 + p.B4 + p.B5 + p.B6 + p.B7));
			if (!Hid.HidD_SetFeature(this.device.Handle, ref buf[0], 9))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		public Packet Get()
		{
			Span<byte> buf = stackalloc byte[9];
			if (!Hid.HidD_GetFeature(this.device.Handle, ref buf[0], 9))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			Packet p;
			p.B0 = buf[0];
			p.B1 = buf[1];
			p.B2 = buf[2];
			p.B3 = buf[3];
			p.B4 = buf[4];
			p.B5 = buf[5];
			p.B6 = buf[6];
			p.B7 = buf[7];
			return p;
		}

		public struct Packet
		{
			public byte B0; // Always 0 it seems.
			public byte B1; // This is the command byte.
			public byte B2;
			public byte B3;
			public byte B4;
			public byte B5;
			public byte B6;
			public byte B7;
		}
	}
}
