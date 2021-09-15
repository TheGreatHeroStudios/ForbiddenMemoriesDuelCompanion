using FMDC.Model;
using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class MonsterTypeToImageCoverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return string.Empty;
			}

			MonsterType monsterType = Enum.Parse<MonsterType>(value.ToString());

			return
				ApplicationConstants.APPLICATION_DATA_FOLDER +
				ApplicationConstants.MONSTER_TYPE_IMAGE_SUBDIRECTORY +
				$"{monsterType}.png";
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
