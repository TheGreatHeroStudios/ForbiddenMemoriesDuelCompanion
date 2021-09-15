using FMDC.Model.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FMDC.Model.Models
{
	public class CardCount : ObservableModel
	{
		#region Propert(ies)
		private int _cardId;
		public int CardId 
		{ 
			get => _cardId; 
			set
			{
				_cardId = value;

			}
		}
		public int NumberInTrunk { get; set; }
		public int NumberInDeck { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public Card Card { get; set; }
		#endregion
	}
}
