using FMDC.Model.Base;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.TestApp.Enums;
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
		#endregion



		#region Public Propertie(s)
		public ObservableCollection<CardCount> CardCounts { get; set; }

		public int DeckCount => CardCounts.Sum(cardCount => cardCount.NumberInDeck);

		public int TrunkCount => CardCounts.Sum(cardCount => cardCount.NumberInTrunk);

		public Card InspectedCard { get; set; }

		public bool CardInspectorOpen => InspectedCard != null;

		public SortMethod SortMethod { get; set; }

		public FilterMethod FilterMethod { get; set; }

		public ObservableCollection<CardCount> FilteredCardCounts
		{
			get
			{
				switch(FilterMethod)
				{
					case FilterMethod.Trunk:
					{
						return
							new ObservableCollection<CardCount>
							(
								CardCounts
									.Where
									(
										cardCount =>
											cardCount.NumberInTrunk > 0
									)
							);
					}

					case FilterMethod.Deck:
					{
						return
							new ObservableCollection<CardCount>
							(
								CardCounts
									.Where
									(
										cardCount =>
											cardCount.NumberInDeck > 0
									)
							);
					}

					default:
					{
						return CardCounts;
					}
				}
			}
		}
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
					.OrderBy
					(
						card => 
							card.CardId
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
			RaisePropertyChanged(nameof(TrunkCount));
		}


		public void SetSortMethod(SortMethod newSortMethod)
		{
			switch(newSortMethod)
			{
				case SortMethod.Number:
				{
					CardCounts =
						new ObservableCollection<CardCount>
						(
							CardCounts
								.OrderBy
								(
									cardCount =>
										cardCount.Card.CardId
								)
						);

					break;
				}
				case SortMethod.Name:
				{
					CardCounts =
						new ObservableCollection<CardCount>
						(
							CardCounts
								.OrderBy
								(
									cardCount =>
										cardCount.Card.Name
								)
						);

					break;
				}
				case SortMethod.Attack:
				{
					CardCounts =
						new ObservableCollection<CardCount>
						(
							CardCounts
								.OrderByDescending
								(
									cardCount =>
										cardCount.Card.AttackPoints ?? -1
								)
						);

					break;
				}
				case SortMethod.Defense:
				{
					CardCounts =
						new ObservableCollection<CardCount>
						(
							CardCounts
								.OrderByDescending
								(
									cardCount =>
										cardCount.Card.DefensePoints ?? -1
								)
						);

					break;
				}
				case SortMethod.Type:
				{
					CardCounts =
						new ObservableCollection<CardCount>
						(
							CardCounts
								.OrderBy
								(
									cardCount =>
										(int)(cardCount.Card.MonsterType ?? MonsterType.Unknown)
								)
								.ThenBy
								(
									cardCount =>
										cardCount.Card.CardType
								)
						);

					break;
				}
			}

			SortMethod = newSortMethod;

			RaisePropertyChanged(nameof(CardCounts));
			RaisePropertyChanged(nameof(FilteredCardCounts));
			RaisePropertyChanged(nameof(SortMethod));
		}


		public void SetFilterMethod(FilterMethod newFilterMethod)
		{
			if (newFilterMethod == FilterMethod)
			{
				//If the filter method supplied is the one that is
				//already set, reset the filter method back to 'None'.
				FilterMethod = FilterMethod.None;
			}
			else
			{
				FilterMethod = newFilterMethod;
			}

			RaisePropertyChanged(nameof(FilteredCardCounts));
			RaisePropertyChanged(nameof(FilterMethod));
		}
		#endregion
	}
}
