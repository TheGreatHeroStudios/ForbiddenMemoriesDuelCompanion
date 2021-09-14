using FMDC.Model;
using FMDC.Model.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class CardToImagePathConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Card card = value as Card;

			return
				card == null || card.CardImage == null ?
					ApplicationConstants.APPLICATION_DATA_FOLDER +
						ApplicationConstants.DEFAULT_THUMBNAIL_RELATIVE_FILEPATH :
					ApplicationConstants.APPLICATION_DATA_FOLDER +
						(value as Card).CardImage.ImageRelativePath;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
