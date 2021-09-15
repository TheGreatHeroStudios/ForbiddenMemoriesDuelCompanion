using FMDC.Model.Enums;
using FMDC.Model.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TGH.Common.Extensions;

namespace FMDC.TestApp.ViewModels
{
	public class TrunkViewModel
	{
		#region Non-Public Member(s)
		private List<Card> _cardList;
		private List<GameImage> _monsterTypeImages;
		#endregion



		#region Public Propertie(s)

		public ObservableCollection<CardCount> CardCounts { get; set; }
		#endregion



		#region Constructor(s)
		public TrunkViewModel(App currentAppInstance)
		{
			_cardList = currentAppInstance.CardList;

			_monsterTypeImages =
				currentAppInstance.GameImages
					.Where
					(
						gameImage =>
							gameImage.EntityType == ImageEntityType.MonsterType
					)
					.ToList();

			LoadCardCounts();
		}
		#endregion



		#region Non-Public Method(s)
		private void LoadCardCounts()
		{
			//TODO: Load this information from a file
			CardCounts =
				new ObservableCollection<CardCount>
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
	}
}
