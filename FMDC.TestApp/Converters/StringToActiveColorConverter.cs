using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace FMDC.TestApp.Converters
{
	public class StringToActiveColorConverter : MarkupExtension, IValueConverter
	{
		#region Public Propert(ies)
		public Brush ActiveColorBrush { get; set; }
		public Brush InactiveColorBrush { get; set; }
		#endregion



		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isActive =
				value.ToString().Equals(parameter.ToString());

			if(isActive)
			{
				return ActiveColorBrush;
			}
			else
			{
				return InactiveColorBrush;
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
