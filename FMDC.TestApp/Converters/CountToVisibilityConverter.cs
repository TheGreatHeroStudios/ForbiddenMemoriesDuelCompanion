using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class CountToVisibilityConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int providedCount = int.Parse(value.ToString());
			int requiredCount = int.Parse(parameter.ToString());

			if(providedCount == requiredCount)
			{
				return Visibility.Visible;
			}
			else
			{
				return Visibility.Collapsed;
			}
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
