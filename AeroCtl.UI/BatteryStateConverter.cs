using System;
using System.Globalization;
using System.Windows.Data;
using AeroCtl.Native;

namespace AeroCtl.UI
{
	public class BatteryStateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is BatteryState state)
			{
				switch (state)
				{
					case BatteryState.NoBattery:
						return "No battery";
					case BatteryState.AC:
						return "AC";
					case BatteryState.DC:
						return "Battery";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}