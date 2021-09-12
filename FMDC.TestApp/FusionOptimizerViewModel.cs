using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TGH.Common.Repository.Interfaces;

namespace FMDC.TestApp
{
	public class FusionOptimizerViewModel : INotifyPropertyChanged
	{
		#region Non-Public Member(s)
		private IGenericRepository _cardRepository;
		private List<Card> _cardList;
		private List<Fusion> _fusionList;

		private Card[] _handCards = new Card[5];
		private Card[] _fieldCards = new Card[5];
		#endregion



		#region Public Propert(ies)
		public List<Card> CardList => _cardList;

		private ObservableCollection<Card> _handCardCollection;
		public ObservableCollection<Card> HandCards { get; set; }
		public ObservableCollection<Card> FieldCards { get; set; }
		#endregion



		#region Constructor(s)
		public FusionOptimizerViewModel(IGenericRepository cardRepository)
		{
			_cardRepository = cardRepository;

			IEnumerable<GameImage> cardThumbnails =
				_cardRepository
					.RetrieveEntities<GameImage>
					(
						gameImage => 
							gameImage.EntityType == ImageEntityType.Card
					);

			_cardList = new List<Card>();
			_cardList.Add
			(
				new Card
				{
					CardId = -1,
					Name = "None"
				}
			);

			_cardList.AddRange
			(
				_cardRepository
					.RetrieveEntities<Card>(card => true)
					.Join
					(
						cardThumbnails,
						card => card.CardImageId,
						thumbnail => thumbnail.GameImageId,
						(card, thumbnail) =>
						{
							card.CardImage = thumbnail;
							return card;
						}
					)
					.OrderBy(card => card.Name)
					.ToList()
			);

			_fusionList = 
				_cardRepository
					.RetrieveEntities<Fusion>(fusion => true)
					.ToList();
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Public Method(s)
		public void UpdateCardSelection(Card updatedCard, bool handCardUpdated, int index)
		{
			if(handCardUpdated)
			{
				_handCards[index] = updatedCard;
				HandCards = new ObservableCollection<Card>(_handCards);
				RaisePropertyChanged(nameof(HandCards));
			}
			else
			{
				_fieldCards[index] = updatedCard;
				FieldCards = new ObservableCollection<Card>(_fieldCards);
				RaisePropertyChanged(nameof(FieldCards));
			}
		}
		#endregion



		#region Non-Public Method(s)
		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?
				.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
