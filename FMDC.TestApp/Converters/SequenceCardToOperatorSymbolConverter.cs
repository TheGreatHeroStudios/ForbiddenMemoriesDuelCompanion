using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace FMDC.TestApp.Converters
{
	public class SequenceCardToOperatorSymbolConverter : MarkupExtension, IValueConverter
	{
		#region Public Propertie(s)
		public int CurrentCardIndex { get; set; }
		#endregion



		#region 'IValueConverter' Implementation
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ObservableCollection<Card> playSequence = value as ObservableCollection<Card>;

			if (playSequence == null || !playSequence.Any())
			{
				//If the sequence is invalid or empty, return an empty string
				return string.Empty;
			}

			Card currentCard =
				CurrentCardIndex < playSequence.Count ?
					playSequence[CurrentCardIndex] :
					null;

			if (currentCard == null)
			{
				return string.Empty;
			}

			Card nextCard =
				CurrentCardIndex + 1 < playSequence.Count ?
					playSequence[CurrentCardIndex + 1] :
					null;

			if (nextCard == null)
			{
				//If the current card is the last overall in the sequence,
				//there are no subsequent operators so return an empty string.
				return string.Empty;
			}
			else if (nextCard.CardType != CardType.Monster)
			{
				//If the next card is not a monster, return the '(E)' operator which
				//signifies that the next card equips the resultant card in the sequence.
				return "(E)";
			}
			else
			{
				//If the next card is a monster, look one further card ahead in 
				//the sequence to determine the next card's relative position.
				Card subsequentCard =
					CurrentCardIndex + 2 < playSequence.Count ?
						playSequence[CurrentCardIndex + 2] :
						null;

				if (subsequentCard == null || subsequentCard.CardType != CardType.Monster)
				{
					//If the next card is the last, or is followed by a non-monster,
					//the next card is the resultant card so return a '=' operator.
					return "=";
				}
				else
				{
					//If there is at least one subsequent monster card
					//after the next card, return a '+' operator.
					return "+";
				}
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
