using FMDC.Model.Base;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TGH.Common.Extensions;

namespace FMDC.TestApp.ViewModels
{
	public class TrunkViewModel : ObservableModel
	{
		#region Non-Public Member(s)
		private List<Card> _cardList;
		private List<GameImage> _monsterTypeImages;
		#endregion



		#region Public Propertie(s)
		public ObservableCollection<CardCount> CardCounts { get; set; }

		public int DeckCount => CardCounts.Sum(cardCount => cardCount.NumberInDeck);
		#endregion



		#region Constructor(s)
		public TrunkViewModel(App currentAppInstance)
		{
			_cardList = 
				currentAppInstance.CardList
					.Where
					(
						card =>
							card.CardId > 0
					)
					.ToList();

			_monsterTypeImages =
				currentAppInstance.GameImages
					.Where
					(
						gameImage =>
							gameImage.EntityType == ImageEntityType.MonsterType
					)
					.ToList();

			LoadCardCounts
			(
				_cardList
					.Select
					(
						card =>
							new CardCount
							{
								Card = card,
								CardId = card.CardId,
								NumberInTrunk = 0,
								NumberInDeck = 0
							}
					)
			);
		}
		#endregion



		#region Public Method(s)
		public void LoadCardCounts(IEnumerable<CardCount> cardCounts)
		{
			SetPropertyValue
			(
				nameof(CardCounts),
				new ObservableCollection<CardCount>(cardCounts)
			);

			RaisePropertyChanged(nameof(DeckCount));
		}
		#endregion
	}
}
