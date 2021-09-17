using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace FMDC.TestApp.Converters
{
	public class CountToVisibilityConverter : MarkupExtension, IValueConverter
	{
		#region Public Propertie(s)
		public bool InvertLogic { get; set; }
		#endregion



		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int providedCount = int.Parse(value.ToString());
			int requiredCount = int.Parse(parameter.ToString());

			if(providedCount == requiredCount)
			{
				return 
					InvertLogic ?
						Visibility.Collapsed :
						Visibility.Visible;
			}
			else
			{
				return
					InvertLogic ?
						Visibility.Visible :
						Visibility.Collapsed;
			}
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion



		#region Abstract Implementation
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
		#endregion
	}
}
