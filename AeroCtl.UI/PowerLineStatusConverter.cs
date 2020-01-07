using System;
using System.Globalization;
using System.Windows.Data;
using AeroCtl.Native;

namespace AeroCtl.UI
{
	public class PowerLineStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is PowerLineStatus status)
			{
				switch (status)
				{
					case PowerLineStatus.Offline:
						return "Battery";
					case PowerLineStatus.Online:
						return "AC";
					default:
						return "?";
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