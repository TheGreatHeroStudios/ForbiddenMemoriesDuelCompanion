using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace FMDC.TestApp.Converters
{
	public class EnumStringConverter : IValueConverter
	{
		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter != null)
			{
				//If a separator was specified, copy each character of the string representation
				//of the enum and insert the delimiter before every case change.
				char delimiter = parameter.ToString()[0];
				string enumString = value.ToString();
				List<char> delimitedString = new List<char>();

				for (int i = 0; i < enumString.Length; i++)
				{
					delimitedString.Add(enumString[i]);

					//If the case of the character is lower and the case of the
					//next character is upper, insert the delimiter character.
					bool caseChanging =
						i < enumString.Length - 1 &&
						char.IsLower(enumString[i]) &&
						char.IsUpper(enumString[i + 1]);

					if (caseChanging)
					{
						delimitedString.Add(delimiter);
					}
				}

				return new string(delimitedString.ToArray());
			}
			else
			{
				return
					value.ToString();
			}
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
