using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class GuardianStarSymbolConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			GuardianStar? guardianStar = value as GuardianStar?;

			if (!guardianStar.HasValue || guardianStar.Value == GuardianStar.Unknown)
			{
				return string.Empty;
			}

			string symbolLookupString = " ☉☾☿♀♂♃♄⛢♆♇";

			return symbolLookupString[(int)guardianStar.Value];
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
