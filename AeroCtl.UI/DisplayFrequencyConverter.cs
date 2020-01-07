using System;
using System.Globalization;
using System.Windows.Data;

namespace AeroCtl.UI
{
	public class DisplayFrequencyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is uint v)
			{
				if (v == 0)
					return "No change";

				return $"{v} Hz";
			}

			return "?";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}