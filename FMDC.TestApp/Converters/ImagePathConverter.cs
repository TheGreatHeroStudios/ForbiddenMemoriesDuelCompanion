using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class ImagePathConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Card card = value as Card;
			Character character = value as Character;

			string imagePath = string.Empty;
			ImageEntityType imageType = ImageEntityType.Card;

			if (parameter != null)
			{
				//If a parameter was provided, parse it as an image entity type enum
				//and use the parsed value to determine which image path to resolve
				imageType = Enum.Parse<ImageEntityType>(parameter.ToString());
			}

			if (imageType == ImageEntityType.Card)
			{
				//For 'Card' images, use the thumbnail filepath
				//(or the default thumbnail path if no image is available)
				imagePath =
					card == null || card.CardImage == null ?
						ApplicationConstants.APPLICATION_DATA_FOLDER + 
							ApplicationConstants.DEFAULT_THUMBNAIL_RELATIVE_FILEPATH :
						ApplicationConstants.APPLICATION_DATA_FOLDER + 
							card.CardImage.ImageRelativePath;
			}
			else if(imageType == ImageEntityType.CardDetails)
			{
				//For 'CardDetail' images, use the description image 
				//filepath (or an empty string if no image is available)
				imagePath =
					card == null || card.CardImage == null ?
						string.Empty :
						ApplicationConstants.APPLICATION_DATA_FOLDER + 
							card.CardDescriptionImage.ImageRelativePath;
			}
			else if(imageType == ImageEntityType.Character)
			{
				//For 'Character' images, use the character image 
				//filepath (or an empty string if no image is available)
				imagePath =
					character == null || character.CharacterImage == null ?
						string.Empty :
						ApplicationConstants.APPLICATION_DATA_FOLDER +
							character.CharacterImage.ImageRelativePath;
			}

			return imagePath;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
