using System;
using System.Threading.Tasks;

namespace AeroCtl
{
	public interface IRgbController
	{
		/// <summary>
		/// Reads the keyboard firmware version.
		/// </summary>
		/// <returns></returns>
		ValueTask<Version> GetFirmwareVersionAsync();
		
		/// <summary>
		/// Sets the RGB light effect.
		/// </summary>
		/// <param name="effect"></param>
		/// <returns></returns>
		ValueTask SetEffectAsync(RgbEffect effect);
		
		/// <summary>
		/// Gets the current light effect.
		/// </summary>
		/// <returns></returns>
		ValueTask<RgbEffect> GetEffectAsync();

		/// <summary>
		/// Sets the image for custom effects.
		/// </summary>
		/// <param name="index">The custom effect index, corresponds to the "Custom" entries in <see cref="RgbEffectType"/>.</param>
		/// <param name="image">The RGB values (4 bytes per pixel/key).</param>
		/// <returns></returns>
		ValueTask SetImageAsync(int index, ReadOnlyMemory<byte> image);
	}
}