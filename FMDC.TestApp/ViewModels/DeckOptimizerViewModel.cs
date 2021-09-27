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
		private List<Fusion> _viableFusions;

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
			_viableFusions = 
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

				if(_availableCardCounts.ContainsKey(optimalCard))
				{
					//If the player has at least one of the card in their current 
					//deck or trunk, add the total number to the optimized deck 
					//(until the deck is full or the available count is exhausted)
					int numberAvailable = _availableCardCounts[optimalCard];

					deckCount =
						AddToOptimizedDeck
						(
							optimalCard,
							ref numberAvailable
						);

					//Update the number left after adding to the optimized deck
					_availableCardCounts[optimalCard] = numberAvailable;
				}
				else if(_viableFusions.Any(fusion => fusion.ResultantCard.Equals(optimalCard)))
				{
					//If the player does not have the card itself in their deck or trunk,
					//but there is a viable way to make it based on the cards in their
					//deck or trunk, use the viable fusions to build an optimal fusion tree.
					BinaryTreeNode<Card> fusionRootNode = new BinaryTreeNode<Card>(optimalCard);

					bool fusionPossible =
						_fusionService
							.BuildFusionTree
							(
								fusionRootNode,
								_viableFusions,
								new Dictionary<Card, int>(_availableCardCounts)
							);

					if(fusionPossible)
					{
						//If a fusion tree was successfully generated for the optimal card
						//(where all leaf node cards are owned by the player), aggregate
						//the leaf nodes into card counts and deduct them from the player's.
					}
				}

				//If adding the last card allowed the optimized
				//deck to reach 40 cards, break out of the loop
				if (deckCount == 40)
				{
					break;
				}
			}
		}
		#endregion



		#region Non-Public Method(s)
		private int AddToOptimizedDeck(Card card, ref int maxCount)
		{
			//Add the specified number of instances of the card
			//to the optimized deck until the deck reaches a max
			//of 40 cards or the max count has been exhausted.
			while(maxCount > 0 && _optimizedDeck.Count < 40)
			{
				_optimizedDeck.Add(card);
			}

			//Return the number of cards currently in the optimized deck
			return _optimizedDeck.Count;
		}
		#endregion
	}
}
