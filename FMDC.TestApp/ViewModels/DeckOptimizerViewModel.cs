using FMDC.BusinessLogic;
using FMDC.Model.Base;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGH.Common.DataStructures;
using TGH.Common.DataStructures.Enums;
using TGH.Common.Extensions;

namespace FMDC.TestApp.ViewModels
{
	public class DeckOptimizerViewModel : ObservableModel
	{
		#region Non-Public Member(s)
		private IFusionService _fusionService;

		private List<Card> _cardList;
		private Dictionary<Card, int> _availableCardCounts;
		private List<Fusion> _viableSpecificFusions;

		private List<Card> _optimizedDeck;
		#endregion



		#region Constructor(s)
		public DeckOptimizerViewModel
		(
			App currentAppInstance,
			IFusionService fusionService
		)
		{
			_fusionService = fusionService;

			_cardList = currentAppInstance.CardList;

			_optimizedDeck = new List<Card>();
		}
		#endregion



		#region Public Method(s)
		public void RefreshAvailableCards(IEnumerable<CardCount> cardCounts)
		{
			_availableCardCounts =
				cardCounts
					.Where
					(
						cardCount =>
							cardCount.NumberInDeck > 0 ||
							cardCount.NumberInTrunk > 0
					)
					.ToDictionary
					(
						cardCount => cardCount.Card,
						cardCount =>
							cardCount.NumberInDeck + cardCount.NumberInTrunk
					);

			//Recursively map each specific fusion the player is
			//currently able to make and cache it for later use.
			_viableSpecificFusions = 
				_fusionService.DetermineViableSpecificFusions(_availableCardCounts);

			//TODO (TEST): Remove the following method call
			OptimizeDeck();
		}


		public void OptimizeDeck()
		{
			//Iterate over cards in order of attack (descending) and
			//determine whether to factor them into deck inclusion
			foreach(Card optimalCard in _cardList.OrderByDescending(card => card.AttackPoints))
			{
				int deckCount = _optimizedDeck.Count;

				if
				(
					_availableCardCounts.ContainsKey(optimalCard) &&
					_availableCardCounts[optimalCard] > 0
				)
				{
					//If the player has at least one of the card in their current 
					//deck or trunk, add the total number to the optimized deck 
					//(until the deck is full or the available count is exhausted)
					int numberAvailable = _availableCardCounts[optimalCard];

					deckCount =
						AddToOptimizedDeck
						(
							optimalCard,
							numberAvailable
						);

					//If adding the last card allowed the optimized
					//deck to reach 40 cards, break out of the loop
					if (deckCount == 40)
					{
						break;
					}

					//Update the number left after adding to the optimized deck
					_availableCardCounts[optimalCard] = 0;
				}

				//While viable fusions resulting in the optimal card exist,
				//try to add cards for each of them to the optimal deck...
				while (_viableSpecificFusions.Any(fusion => fusion.ResultantCard.Equals(optimalCard)))
				{
					BinaryTreeNode<Card> fusionRootNode = new BinaryTreeNode<Card>(optimalCard);

					List<Fusion> currentViableFusions = new List<Fusion>(_viableSpecificFusions);

					bool fusionPossible =
						_fusionService
							.BuildFusionTree
							(
								fusionRootNode,
								currentViableFusions,
								new Dictionary<Card, int>(_availableCardCounts)
							);

					if (fusionPossible)
					{
						//If a fusion tree was successfully generated for the optimal card
						//(where all leaf node cards are owned by the player), aggregate
						//the leaf nodes into card counts and deduct them from the player's.
						IEnumerable<IGrouping<Card, Card>> requiredCardCounts =
							fusionRootNode
								.GetChildNodes()
								.Where
								(
									node =>
										node.IsLeafNode
								)
								.Select
								(
									node =>
										node.Data
								)
								.GroupBy
								(
									card =>
										card
								);

						//If a fusion is possible, update the list of viable fusions
						//to reflect those expended while generating the current card
						_viableSpecificFusions = currentViableFusions;

						foreach (IGrouping<Card, Card> requiredCardCount in requiredCardCounts)
						{
							int requiredCount = requiredCardCount.Count();

							//Add the required card(s) to the deck
							deckCount =
								AddToOptimizedDeck
								(
									requiredCardCount.Key,
									requiredCount
								);

							//If adding the last card allowed the optimized
							//deck to reach 40 cards, break out of the loop
							if (deckCount == 40)
							{
								break;
							}

							_availableCardCounts[requiredCardCount.Key] -= requiredCount;
						}
					}
					else
					{
						//If a fusion is not possible, break out of the loop
						break;
					}
				}

				//Once all specific fusions for the card have been exhausted,
				//retrieve any general fusions that can be made for the card
				//and add the necessary material cards to the deck one-by-one.
				foreach
				(
					Card generalMaterialCard in 
					_fusionService
						.GetGeneralFusionCards
						(
							optimalCard,
							_availableCardCounts
						)
				)
				{
					deckCount = AddToOptimizedDeck(generalMaterialCard, 1);

					//If adding the last card allowed the optimized
					//deck to reach 40 cards, break out of the inner loop
					if (deckCount == 40)
					{
						break;
					}
				}

				//If adding the general fusions allowed the optimized
				//deck to reach 40 cards, break out of the outer loop
				if (deckCount == 40)
				{
					break;
				}
			}
		}
		#endregion



		#region Non-Public Method(s)
		private int AddToOptimizedDeck(Card card, int count)
		{
			//Add the specified number of instances of the card
			//to the optimized deck until the deck reaches a max
			//of 40 cards or the max count has been exhausted.
			while (count > 0 && _optimizedDeck.Count < 40)
			{
				_optimizedDeck.Add(card);
				count--;
			}
			
			//Return the number of cards currently in the optimized deck
			return _optimizedDeck.Count;
		}
		#endregion
	}
}
