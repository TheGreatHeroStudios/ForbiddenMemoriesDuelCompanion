using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class EnumVisibilityConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (int.Parse(parameter.ToString()) == (int)value)
			{
				return Visibility.Visible;
			}

			return Visibility.Collapsed;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
