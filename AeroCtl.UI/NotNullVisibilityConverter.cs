﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AeroCtl.UI
{
	public class NotNullVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

}