using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FMDC.TestApp
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool invertLogic = false;

			if (parameter != null)
			{
				invertLogic = bool.Parse(parameter.ToString());
			}

			bool inputFlag = bool.Parse(value.ToString());

			if (invertLogic)
			{
				inputFlag = !inputFlag;
			}

			if (inputFlag)
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
