using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IKeyboardRgbController
	{
		Task<Version> GetFirmwareVersionAsync();
		Task SetEffectAsync(LightEffect effect, int speed, int brightness, LightColor color, int direction);
		Task SetImageAsync(int index, ReadOnlyMemory<byte> image);
	}
}