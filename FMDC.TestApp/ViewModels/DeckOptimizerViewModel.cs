﻿using FMDC.BusinessLogic;
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
		private List<CardPercentage> _availableCardDrops;
		private Dictionary<Card, CardCount> _trunkCardCounts;
		private Dictionary<Card, int> _availableCardCounts;
		private List<Fusion> _viableSpecificFusions;

		private List<Card> _currentDeck;
		private List<Card> _optimizedDeck;
		private List<BinaryTreeNode<Card>> _nonPossibleFusions;
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

		public bool OptimizerStrategyAvailable => OptimizerStrategy?.Any() ?? false;

		public ObservableCollection<OptimizerSuggestion> OptimizerStrategy { get; set; }

		public List<OptimizerSuggestion> SelectedOptimizerSuggestions { get; set; }

		public bool OptimizerSuggestionsSelected => (SelectedOptimizerSuggestions?.Count ?? 0) > 0;

		public ObservableCollection<ObservableCollection<Card>> OptimalFusions { get; set; }

		public bool OptimalFusionWindowOpen { get; set; }

		public bool IncludeNonMonstersInOptimization { get; set; }

		public CardPercentage GrindCardPercentage { get; set; }

		public bool GrindCardAvailable => GrindCardPercentage != null;
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
			_availableCardDrops = currentAppInstance.CardDropPercentages;

			_currentDeck = new List<Card>();
			_optimizedDeck = new List<Card>();
			_nonPossibleFusions = new List<BinaryTreeNode<Card>>();

			OptimizerStrategy = new ObservableCollection<OptimizerSuggestion>();
			OptimalFusions = new ObservableCollection<ObservableCollection<Card>>();
		}
		#endregion



		#region Public Method(s)
		public void RefreshAvailableCards()
		{
			RefreshAvailableCards
			(
				_trunkCardCounts
					.Select(trunkCardCount => trunkCardCount.Value)
			);
		}


		public void RefreshAvailableCards(IEnumerable<CardCount> cardCounts)
		{
			//Reset any values set during previous optimizations
			_currentDeck.Clear();
			_optimizedDeck.Clear();
			_nonPossibleFusions.Clear();
			OptimizerStrategy.Clear();
			OptimalFusions.Clear();

			OptimizedFusionCount = 0;
			TotalOptimalFusionStrength = 0;
			TotalFusionMaterialCardsNecessary = 0;
			TotalFusionPermutations = 0;

			OptimalFusionWindowOpen = false;

			_trunkCardCounts = 
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
						cardCount => cardCount
					);

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
			foreach
			(
				CardCount deckCardCount in 
				cardCounts
					.Where
					(
						cardCount => 
							cardCount.NumberInDeck > 0
					)
			)
			{
				AddToDeck
				(
					_currentDeck,
					deckCardCount.Card,
					deckCardCount.NumberInDeck,
					40
				);
			}

			//Recursively map each specific fusion the player is
			//currently able to make and cache it for later use.
			_viableSpecificFusions = 
				_fusionService.DetermineViableSpecificFusions(_availableCardCounts);

			//TODO (TEST): Move the following method calls
			OptimizeDeck();
			BuildOptimizerStrategy();
			DetermineGrindCardPercentage();
		}


		public void OptimizeDeck()
		{
			int optimizedDeckCount = 0;

			//If non monsters should be included in the optimization strategy, build an optimal deck of 40 cards.
			//Otherwise, build an optimal deck of 40 minus the number of magic and trap cards in the current deck.
			int targetDeckCount =
				IncludeNonMonstersInOptimization ?
					40 :
					40 - _currentDeck.Count(card => card.CardType != CardType.Monster);

			//Iterate over cards in order of attack (descending) and
			//determine whether to factor them into deck inclusion
			foreach (Card optimalCard in _cardList.OrderByDescending(card => card.AttackPoints))
			{
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

					optimizedDeckCount =
						AddToDeck
						(
							_optimizedDeck,
							optimalCard,
							numberAvailable,
							targetDeckCount
						);

					//If adding the last card allowed the optimized deck to
					//reach the target number of cards, break out of the loop
					if (optimizedDeckCount == targetDeckCount)
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
							optimizedDeckCount = 
								AddToDeck
								(
									_optimizedDeck, 
									requiredCard, 
									1,
									targetDeckCount
								);

							//If adding the last card allowed the optimized deck to
							//reach the target number of cards, break out of the loop
							if (optimizedDeckCount == targetDeckCount)
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
						//If one or more viable fusions exists but is not possible given
						//the player's current set of cards, cache the incomplete fusion tree
						_nonPossibleFusions.Add(fusionRootNode);
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
					optimizedDeckCount = 
						AddToDeck
						(
							_optimizedDeck, 
							generalMaterialCard, 
							1,
							targetDeckCount
						);

					//If adding the last card allowed the optimized deck to
					//reach the target number of cards, break out of the inner loop
					if (optimizedDeckCount == targetDeckCount)
					{
						break;
					}
				}

				//If adding the general fusions allowed the optimized deck to
				//reach the target number of cards, break out of the outer loop
				if (optimizedDeckCount == targetDeckCount)
				{
					break;
				}
			}

			RaisePropertyChanged(nameof(OptimizedFusionCount));
			RaisePropertyChanged(nameof(OptimalFusions));
			RaisePropertyChanged(nameof(TotalOptimalFusionStrength));
			RaisePropertyChanged(nameof(AverageOptimalFusionStrength));
			RaisePropertyChanged(nameof(TotalFusionMaterialCardsNecessary));
			RaisePropertyChanged(nameof(AverageFusionMaterialCardsNecessary));
			RaisePropertyChanged(nameof(AverageFusionMaterialStrength));
			RaisePropertyChanged(nameof(TotalFusionPermutations));
			RaisePropertyChanged(nameof(AverageCardStrength));
		}
		
		
		public void UpdateSelectedOptimizationSuggestions
		(
			IEnumerable<OptimizerSuggestion> selectedSuggestions
		)
		{
			SetPropertyValue
			(
				nameof(SelectedOptimizerSuggestions),
				selectedSuggestions.ToList()
			);

			RaisePropertyChanged(nameof(OptimizerSuggestionsSelected));
		}


		public void ApplyOptimizations
		(
			List<OptimizerSuggestion> acceptedOptimizations
		)
		{
			for(int i = acceptedOptimizations.Count - 1; i >= 0; i--)
			{
				OptimizerSuggestion optimization = acceptedOptimizations[i];
				CardCount targetCardCount =
						_trunkCardCounts[optimization.TargetCard];

				if (optimization.Direction == OptimizerDirection.ToDeck)
				{
					targetCardCount.NumberInDeck += optimization.Amount;
					targetCardCount.NumberInTrunk -= optimization.Amount;
				}
				else if(optimization.Direction == OptimizerDirection.ToTrunk)
				{
					targetCardCount.NumberInDeck -= optimization.Amount;
					targetCardCount.NumberInTrunk += optimization.Amount;
				}

				OptimizerStrategy.Remove(optimization);
			}

			RaisePropertyChanged(nameof(OptimizerStrategy));
			RaisePropertyChanged(nameof(OptimizerStrategyAvailable));
		}
		#endregion



		#region Non-Public Method(s)
		private int AddToDeck
		(
			List<Card> targetDeck, 
			Card card, 
			int count,
			int maxDeckSize
		)
		{
			//Add the specified number of instances of the card to
			//the optimized deck until the deck reaches the maximum
			//number of cards or the available count has been exhausted.
			while (count > 0 && _optimizedDeck.Count < maxDeckSize)
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

			List<BinaryTreeNode<Card>> optimalFusionNodes =
				fusionRootNode
					.GetChildNodes()
					.Where
					(
						childNode =>
							childNode.IsLeafNode
					)
					.ToList();

			int optimalFusionMaterialCardCount = optimalFusionNodes.Count;

			ObservableCollection<Card> fusionCards = 
				new ObservableCollection<Card>
				(
					optimalFusionNodes
						.Select
						(
							node => 
								node.Data
						)
				);

			fusionCards.Add(fusionRootNode.Data);

			OptimalFusions.Add(fusionCards);

			OptimizedFusionCount++;
			TotalOptimalFusionStrength += optimalCard.AttackPoints ?? 0;
			TotalFusionMaterialCardsNecessary += optimalFusionMaterialCardCount;
			TotalFusionPermutations += distinctFusionCount;
		}
		
		
		private void BuildOptimizerStrategy()
		{
			IEnumerable<OptimizerSuggestion> optimizerStrategy =
				_currentDeck
					.Where
					(
						card =>
							IncludeNonMonstersInOptimization ||
							card.CardType == CardType.Monster
					)
					.GroupBy
					(
						card => card
					)
					
					.FullOuterJoin
					(
						_optimizedDeck
							.Where
							(
								card =>
									IncludeNonMonstersInOptimization ||
									card.CardType == CardType.Monster
							)
							.GroupBy
							(
								card => card
							),
						currentCard => currentCard.Key,
						optimizedCard => optimizedCard.Key,
						(currentCardGroup, optimizedCardGroup) =>
						{
							if(currentCardGroup == null)
							{
								return 
									(
										card: optimizedCardGroup.Key, 
										count: optimizedCardGroup.Count()
									);
							}
							else if (optimizedCardGroup == null)
							{
								return 
									(
										card: currentCardGroup.Key, 
										count: currentCardGroup.Count() * -1
									);
							}
							else
							{
								return 
									(
										card: currentCardGroup.Key, 
										count: optimizedCardGroup.Count() - currentCardGroup.Count()
									);
							}
						}	
					)
					.Where
					(
						optimizerCardCount =>
							optimizerCardCount.count != 0
					)
					.Select
					(
						optimizerCardCount =>
							new OptimizerSuggestion
							{
								Amount = Math.Abs(optimizerCardCount.count),
								Direction = 
									optimizerCardCount.count > 0 ?
										OptimizerDirection.ToDeck :
										OptimizerDirection.ToTrunk,
								TargetCard = optimizerCardCount.card
							}
					);

			OptimizerStrategy =
				new ObservableCollection<OptimizerSuggestion>
				(
					optimizerStrategy
				);

			RaisePropertyChanged(nameof(OptimizerStrategy));
			RaisePropertyChanged(nameof(OptimizerStrategyAvailable));
		}
		
		
		private void DetermineGrindCardPercentage()
		{
			CardPercentage bestGrindCard =
				_availableCardDrops
					.OrderByDescending
					(
						cardDrop =>
							cardDrop.Card.AttackPoints
					)
					.ThenByDescending
					(
						cardDrop =>
							cardDrop.GenerationPercentage
					)
					.FirstOrDefault();

			Card bestPossibleFusionResult = null;
			CardPercentage fusionMaterialGrindCard = null;

			foreach
			(
				BinaryTreeNode<Card> nonPossibleFusionRootNode in 
				_nonPossibleFusions
					.Where
					(
						fusionRootNode =>
							fusionRootNode
								.GetChildNodes()
								.Count
								(
									fusionChildNode =>
										!_availableCardCounts.ContainsKey(fusionChildNode.Data) ||
										_availableCardCounts[fusionChildNode.Data] < 1
								) == 1
					)
					.OrderByDescending
					(
						fusionRootNode =>
							fusionRootNode.Data.AttackPoints ?? 0
					)
			)
			{
				List<Card> necessaryCards =
					nonPossibleFusionRootNode
						.GetChildNodes()
						.Where
						(
							fusionChildNode =>
								!_availableCardCounts.ContainsKey(fusionChildNode.Data) ||
								_availableCardCounts[fusionChildNode.Data] < 1
						)
						.Select
						(
							childNode =>
								childNode.Data
						)
						.ToList();

				if(necessaryCards.None())
				{
					continue;
				}

				if(necessaryCards.All(card => card.In(_availableCardDrops.Select(drop => drop.Card))))
				{
					//If all the necessary cards for the fusion can potentially
					//be dropped by an unlocked character, select the card
					//percentage that has the highest percentage drop rate
					fusionMaterialGrindCard =
						_availableCardDrops
							.Where
							(
								cardDrop =>
									cardDrop.Card.In(necessaryCards)
							)
							.OrderByDescending
							(
								cardDrop =>
									cardDrop.GenerationPercentage
							)
							.First();

					//Also, cache the resultant card which will be compared with the strongest 
					//standalone grind card to determine the best card for which to grind.
					bestPossibleFusionResult = nonPossibleFusionRootNode.Data;

					break;
				}
			}

			if(bestGrindCard != null && fusionMaterialGrindCard != null)
			{
				//If both a standalone grind card and fusion material grind card were 
				//resolved, take the percentage which would result in the stronger monster 
				GrindCardPercentage =
					bestGrindCard.Card.AttackPoints >= (bestPossibleFusionResult?.AttackPoints ?? 0) ?
						bestGrindCard :
						fusionMaterialGrindCard;
			}
			else if(bestGrindCard != null)
			{
				GrindCardPercentage = bestGrindCard;
			}
			else
			{
				GrindCardPercentage = fusionMaterialGrindCard;
			}

			RaisePropertyChanged(nameof(GrindCardPercentage));
			RaisePropertyChanged(nameof(GrindCardAvailable));
		}
		#endregion
	}
}
