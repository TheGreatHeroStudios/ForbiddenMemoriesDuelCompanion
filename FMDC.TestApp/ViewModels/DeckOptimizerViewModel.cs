using FMDC.BusinessLogic;
using FMDC.Model.Base;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TGH.Common.DataStructures;
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

		private List<Card> _currentDeck;
		private List<Card> _optimizedDeck;
		#endregion



		#region Public Propert(ies)
		public float AverageCardStrength =>
			(float)((_optimizedDeck?.Any() ?? false) ?
				_optimizedDeck.Average(card => card.AttackPoints ?? 0) : 0);
		public int OptimizedFusionCount { get; set; }
		public int TotalOptimalFusionStrength { get; set; }
		public float AverageOptimalFusionStrength =>
			OptimizedFusionCount == 0 ?
				0 : (float)TotalOptimalFusionStrength / OptimizedFusionCount;
		public int TotalFusionMaterialCardsNecessary { get; set; }
		public float AverageFusionMaterialCardsNecessary =>
			OptimizedFusionCount == 0 ?
				0 : (float)TotalFusionMaterialCardsNecessary / OptimizedFusionCount;
		public float AverageFusionMaterialStrength =>
			AverageFusionMaterialCardsNecessary == 0 ?
				0 : (float)AverageOptimalFusionStrength / AverageFusionMaterialCardsNecessary;
		public int TotalFusionPermutations { get; set; }

		public ObservableCollection<OptimizerSuggestion> OptimizerStrategy { get; set; }
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
			_currentDeck = new List<Card>();
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

			//Build a list of cards currently in the players deck from provided card counts
			foreach(CardCount deckCardCount in cardCounts.Where(cardCount => cardCount.NumberInDeck > 0))
			{
				AddToDeck
				(
					_currentDeck,
					deckCardCount.Card,
					deckCardCount.NumberInDeck
				);
			}

			//Recursively map each specific fusion the player is
			//currently able to make and cache it for later use.
			_viableSpecificFusions = 
				_fusionService.DetermineViableSpecificFusions(_availableCardCounts);

			//TODO (TEST): Move the following method calls
			OptimizeDeck();
			BuildOptimizerStrategy();
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
						AddToDeck
						(
							_optimizedDeck,
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
						IEnumerable<Card> requiredCards =
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
								);

						//If a fusion is possible, update the list of viable fusions
						//to reflect those expended while generating the current card
						_viableSpecificFusions = currentViableFusions;

						foreach (Card requiredCard in requiredCards)
						{
							//Add the required card to the deck
							deckCount = 
								AddToDeck(_optimizedDeck, requiredCard, 1);

							//If adding the last card allowed the optimized
							//deck to reach 40 cards, break out of the loop
							if (deckCount == 40)
							{
								break;
							}

							_availableCardCounts[requiredCard]--;
						}

						//Add the current fusion tree into the optimizer's aggregate counts
						AddToOptimizedFusionAggregates(fusionRootNode);
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
					deckCount = 
						AddToDeck(_optimizedDeck, generalMaterialCard, 1);

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

			RaisePropertyChanged(nameof(OptimizedFusionCount));
			RaisePropertyChanged(nameof(TotalOptimalFusionStrength));
			RaisePropertyChanged(nameof(AverageOptimalFusionStrength));
			RaisePropertyChanged(nameof(TotalFusionMaterialCardsNecessary));
			RaisePropertyChanged(nameof(AverageFusionMaterialCardsNecessary));
			RaisePropertyChanged(nameof(AverageFusionMaterialStrength));
			RaisePropertyChanged(nameof(TotalFusionPermutations));
			RaisePropertyChanged(nameof(AverageCardStrength));
		}
		#endregion



		#region Non-Public Method(s)
		private int AddToDeck(List<Card> targetDeck, Card card, int count)
		{
			//Add the specified number of instances of the card
			//to the optimized deck until the deck reaches a max
			//of 40 cards or the max count has been exhausted.
			while (count > 0 && _optimizedDeck.Count < 40)
			{
				targetDeck.Add(card);
				count--;
			}
			
			//Return the number of cards currently in the optimized deck
			return _optimizedDeck.Count;
		}
		

		private void AddToOptimizedFusionAggregates(BinaryTreeNode<Card> fusionRootNode)
		{
			Card optimalCard = fusionRootNode.Data;

			int distinctFusionCount =
				fusionRootNode
					.GetChildNodes()
					.Where
					(
						childNode =>
							!childNode.IsLeafNode
					)
					.Count() + 1;

			int optimalFusionMaterialCardCount =
				fusionRootNode
					.GetChildNodes()
					.Where
					(
						childNode =>
							childNode.IsLeafNode
					)
					.Count();

			OptimizedFusionCount++;
			TotalOptimalFusionStrength += optimalCard.AttackPoints ?? 0;
			TotalFusionMaterialCardsNecessary += optimalFusionMaterialCardCount;
			TotalFusionPermutations += distinctFusionCount;
		}
		
		
		private void BuildOptimizerStrategy()
		{
			OptimizerStrategy =
				new ObservableCollection<OptimizerSuggestion>
				(
					_currentDeck
						.GroupBy
						(
							card => card
						)
						.FullOuterJoin
						(
							_optimizedDeck
								.GroupBy
								(
									card => card
								),
							currentCard => currentCard.Key,
							optimizedCard => optimizedCard.Key,
							(currentCardGroup, optimizedCardGroup) =>
							{
								if (currentCardGroup == null)
								{
									//If there is no current card group, the optimized 
									//card is being fully added to the current deck.
									return
										new OptimizerSuggestion
										{
											TargetCard = optimizedCardGroup.Key,
											Direction = OptimizerDirection.ToDeck,
											Amount = optimizedCardGroup.Count()
										};
								}
								else if (optimizedCardGroup == null)
								{
									//If there is no optimized card group, the current
									//card is being fully removed from the current deck
									return
										new OptimizerSuggestion
										{
											TargetCard = currentCardGroup.Key,
											Direction = OptimizerDirection.ToTrunk,
											Amount = currentCardGroup.Count()
										};
								}
								else if (currentCardGroup.Count() > optimizedCardGroup.Count())
								{
									//If there is a greater count of the current card 
									//in the player's current deck than in the optimized  
									//deck, shift the difference to the player's trunk
									return
										new OptimizerSuggestion
										{
											TargetCard = currentCardGroup.Key,
											Direction = OptimizerDirection.ToTrunk,
											Amount = currentCardGroup.Count() - optimizedCardGroup.Count()
										};
								}
								else if (optimizedCardGroup.Count() > currentCardGroup.Count())
								{
									//If there is a greater count of the current card 
									//in the optimized deck than in the player's current 
									//deck, shift the difference to the player's deck
									return
										new OptimizerSuggestion
										{
											TargetCard = currentCardGroup.Key,
											Direction = OptimizerDirection.ToDeck,
											Amount = optimizedCardGroup.Count() - currentCardGroup.Count()
										};
								}
								else
								{
									//If the number of the current card in both the player's current 
									//deck and the optimized deck are the same, no change is necessary
									return
										new OptimizerSuggestion
										{
											TargetCard = currentCardGroup.Key,
											Direction = OptimizerDirection.NoChange,
											Amount = 0
										};
								}
							}
						)
						.Where
						(
							optimization =>
								optimization.Direction != OptimizerDirection.NoChange
						)
				);

			RaisePropertyChanged(nameof(OptimizerStrategy));
		}
		#endregion
	}
}
