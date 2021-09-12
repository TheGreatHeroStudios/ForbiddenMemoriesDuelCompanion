using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace FMDC.TestApp
{
	public class FusionCardCountToOperatorSymbolConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int fusionCardCount = int.Parse(value.ToString());
			int nextCardOrdinal = int.Parse(parameter.ToString());

			if (fusionCardCount == 1)
			{
				//If there is only one card in the fusion sequence, display nothing
				return string.Empty;
			}
			else if (fusionCardCount > nextCardOrdinal)
			{
				//If there are additional cards to display beyond the current ordinal, display a '+'
				return "+";
			}
			else if(fusionCardCount == nextCardOrdinal)
			{
				//If the current ordinal is the last card in the sequence, display a '='
				return "=";
			}
			else
			{
				//If the current ordinal surpasses the number of cards in the sequence, display nothing
				return string.Empty;
			}
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
