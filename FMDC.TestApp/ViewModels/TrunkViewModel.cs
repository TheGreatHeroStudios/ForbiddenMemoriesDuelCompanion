using FMDC.Model.Base;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.TestApp.Enums;
using System;
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


		public void RandomizeCardCounts()
		{
			List<CardCount> randomizedCardCounts = new List<CardCount>();
			Random rng = new Random();

			//Iterate over each card in the game and add a
			//random number of them to the player's trunk
			foreach (Card card in _cardList)
			{
				CardCount currentCardCount =
					new CardCount
					{
						CardId = card.CardId,
						Card = card,
						NumberInDeck = 0,
						NumberInTrunk = 0
					};

				if (card.AttackPoints >= 2500)
				{
					//If the card has more than 2500 attack, give it a
					//10% chance of appearing in the randomized trunk
					int generationValue = rng.Next(0, 100);

					if (generationValue < 10)
					{
						currentCardCount.NumberInTrunk = 1;
					}
				}
				else if (card.AttackPoints >= 1200)
				{
					//If the card has more than 1200 attack, give it a
					//25% chance of appearing in the randomized trunk
					int generationValue = rng.Next(0, 100);

					if (generationValue < 25)
					{
						currentCardCount.NumberInTrunk = 1;
					}
				}
				else
				{
					//If the card has less than 1200 attack points, 
					//spawn between 0 and 2 of each in the trunk.
					int trunkCount = rng.Next(0, 3);

					currentCardCount.NumberInTrunk = trunkCount;
				}

				randomizedCardCounts.Add(currentCardCount);
			}

			SetPropertyValue
			(
				nameof(CardCounts),
				new ObservableCollection<CardCount>(randomizedCardCounts)
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
