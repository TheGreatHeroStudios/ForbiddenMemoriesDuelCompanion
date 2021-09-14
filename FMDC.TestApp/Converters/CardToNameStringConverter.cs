using FMDC.Model.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class CardToNameStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Card card = value as Card;

			return
				card == null ?
					string.Empty :
					card.CardId == -1 ?
						card.Name :
						$"({card.CardId:000}) {card.Name}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
