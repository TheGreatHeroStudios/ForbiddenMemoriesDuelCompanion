using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.TestApp.Comparers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TGH.Common.Extensions;

namespace FMDC.TestApp.ViewModels
{
	public class PlayOptimizerViewModel : INotifyPropertyChanged
	{
		#region Non-Public Member(s)
		private List<Card> _cardList;
		private List<Fusion> _fusionList;
		private List<Equippable> _equippableList;

		private List<Card> _deckList;
		private Card[] _handCards = new Card[5];
		private Card[] _fieldCards = new Card[5];
		private List<Card> _availableEquipCards;
		private List<Card> _availableFieldCards;
		#endregion



		#region Public Propert(ies)
		public ObservableCollection<Card> HandCards { get; set; }
		public ObservableCollection<Card> FieldCards { get; set; }
		public ObservableCollection<Card> OptimalPlay { get; set; }

		public List<Card> CardList => _cardList;

		public List<Card> DeckList => _deckList;

		public List<bool> HandCardEquipmentFlags =>
			_handCards
				.Select
				(
					handCard =>
						_availableEquipCards != null &&
						handCard != null &&
						handCard.In(_availableEquipCards)
				)
				.ToList();

		public List<bool> HandCardFieldFlags =>
			_handCards
				.Select
				(
					handCard =>
						_availableFieldCards != null &&
						handCard != null &&
						handCard.In(_availableFieldCards)
				)
				.ToList();

		public bool EquipCardAvailable =>
			HandCardEquipmentFlags.Any(equipFlag => equipFlag);

		public bool FieldCardAvailable =>
			HandCardFieldFlags.Any(fieldFlag => fieldFlag);

		public bool ModifierCardAvailable =>
			EquipCardAvailable || FieldCardAvailable;

		public string OptimalPlayEnhancedText => ModifyOptimalPlayText();

		public int OptimalPlayCardCount => OptimalPlay?.Count ?? 0;

		public bool ThrowAwayFirstCardInSequence { get; set; }

		public IEnumerable<Card> ValidHandCards =>
			_handCards
				.Where
				(
					handCard =>
						handCard != null &&
						handCard.CardId != -1
				);

		public IEnumerable<Card> ValidFieldCards =>
			_fieldCards
				.Where
				(
					fieldCard =>
						fieldCard != null &&
						fieldCard.CardId != -1
				);

		public bool GenerateFusionEnabled => ValidHandCards?.Any() ?? false;

		public bool AcceptFusionEnabled => OptimalPlay?.Any() ?? false;

		public bool ClearCardDataEnabled =>
			(ValidHandCards?.Any() ?? false) || (ValidFieldCards?.Any() ?? false);
		#endregion



		#region Constructor(s)
		public PlayOptimizerViewModel(App currentAppInstance)
		{
			_cardList = currentAppInstance.CardList;
			_fusionList = currentAppInstance.FusionList;
			_equippableList = currentAppInstance.EquippableList;

			SetPlaceholderCards();
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Public Method(s)
		public void SetPlaceholderCards()
		{
			//Initialize the deck list and add a placeholder card at the start of it
			_deckList = new List<Card>();
			_deckList.Add(_cardList[0]);

			for (int i = 0; i < 5; i++)
			{
				_handCards[i] = _deckList[0];
				_fieldCards[i] = _deckList[0];
			}

			FieldCards = new ObservableCollection<Card>(_fieldCards);
			HandCards = new ObservableCollection<Card>(_handCards);

			RaisePropertyChanged(nameof(FieldCards));
			RaisePropertyChanged(nameof(HandCards));
			RaisePropertyChanged(nameof(GenerateFusionEnabled));
			RaisePropertyChanged(nameof(ClearCardDataEnabled));
		}


		public void RefreshDeckList(IEnumerable<CardCount> cardCounts)
		{
			//Re-initialize the deck list, starting with a placeholder card.
			_deckList = new List<Card>();
			_deckList.Add(_cardList[0]);

			//Load the deck list based on cards from the trunk with
			//a count of one or more instances in the player's deck.
			_deckList.AddRange
			(
				cardCounts
					.Where
					(
						cardCount =>
							cardCount.NumberInDeck > 0
					)
					.Select
					(
						cardCount =>
							cardCount.Card
					)
			);

			RaisePropertyChanged(nameof(DeckList));
		}


		public void UpdateCardSelection(Card updatedCard, bool handCardUpdated, int index)
		{
			if (handCardUpdated)
			{
				_handCards[index] = updatedCard;
				HandCards = new ObservableCollection<Card>(_handCards);

				RaisePropertyChanged(nameof(HandCardFieldFlags));
				RaisePropertyChanged(nameof(FieldCardAvailable));
				RaisePropertyChanged(nameof(HandCardEquipmentFlags));
				RaisePropertyChanged(nameof(EquipCardAvailable));
				RaisePropertyChanged(nameof(ModifierCardAvailable));
				RaisePropertyChanged(nameof(OptimalPlayEnhancedText));
				RaisePropertyChanged(nameof(HandCards));
				RaisePropertyChanged(nameof(GenerateFusionEnabled));
				RaisePropertyChanged(nameof(ClearCardDataEnabled));
			}
			else
			{
				_fieldCards[index] = updatedCard;
				FieldCards = new ObservableCollection<Card>(_fieldCards);

				RaisePropertyChanged(nameof(FieldCards));
				RaisePropertyChanged(nameof(ClearCardDataEnabled));
			}

			ClearOptimalPlayData();
		}


		public void DetermineOptimalPlay()
		{
			//Get a list of all potential fusion permutations available
			//based on the cards in the player's hand and on the field.
			List<List<Card>> potentialFusionPermutations = MapPotentialFusionPermutations();
			List<Card> optimalPlay = new List<Card>();

			if (potentialFusionPermutations.Any())
			{
				//Get a list of equippable(s) where the equip card is currently in the player's 
				//hand and can be applied to one or more of the permutations' resultant cards.
				List<Equippable> potentialEquips =
					_equippableList
						.Join
						(
							potentialFusionPermutations
								.Select(permutation => permutation.Last()),
							equippable => equippable.TargetCardId,
							targetCard => targetCard.CardId,
							(equippable, targetCard) => equippable
						)
						.Where
						(
							equippable =>
								equippable.EquipCard.In(_handCards)
						)
						.ToList();

				//Get a list of field cards in the player's
				//hand and project them to a list of terrains
				List<Terrain> potentialTerrains =
					_handCards
						.Where
						(
							handCard =>
								handCard.CardType == CardType.Field
						)
						.Select
						(
							fieldCard =>
								fieldCard.ToTerrain()
						)
						.ToList();

				//Get the optimal fusion permutation to use
				List<Card> optimalFusionPermutation =
					DetermineOptimalFusionPermutation
					(
						potentialFusionPermutations,
						potentialEquips,
						potentialTerrains
					);

				//Take the optimal permutation and build a play sequence for it
				//(by stripping out intermediate cards from the permutation)
				optimalPlay =
					BuildOptimalPlaySequence(optimalFusionPermutation);

				//Select any equip cards from the player's hand which can be applied to 
				//the optimal resultant card and store them for visualization in the UI
				_availableEquipCards =
					_handCards
						.Where
						(
							equipCard =>
								equipCard.CardType == CardType.Equip &&
								potentialEquips
									.Any
									(
										equip =>
											equip.EquipCard.Equals(equipCard) &&
											equip.TargetCard.Equals(optimalPlay.Last())
									)
						)
						.ToList();

				//Select any field cards from the player's hand which produce terrains
				//favorable to the resultant card and store them for visualization in the UI
				_availableFieldCards =
					_handCards
						.Where
						(
							fieldCard =>
								fieldCard.CardType == CardType.Field &&
								fieldCard
									.ToTerrain()
									.In(optimalPlay.Last().FavorableTerrains())
						)
						.ToList();
			}

			OptimalPlay = new ObservableCollection<Card>(optimalPlay);

			RaisePropertyChanged(nameof(HandCardFieldFlags));
			RaisePropertyChanged(nameof(FieldCardAvailable));
			RaisePropertyChanged(nameof(HandCardEquipmentFlags));
			RaisePropertyChanged(nameof(EquipCardAvailable));
			RaisePropertyChanged(nameof(ModifierCardAvailable));
			RaisePropertyChanged(nameof(OptimalPlayEnhancedText));
			RaisePropertyChanged(nameof(OptimalPlay));
			RaisePropertyChanged(nameof(OptimalPlayCardCount));
			RaisePropertyChanged(nameof(AcceptFusionEnabled));
			RaisePropertyChanged(nameof(ThrowAwayFirstCardInSequence));
		}


		public void AcceptFusion()
		{
			//Determine whether the accepted fusion started with a card from the hand or from the field
			//Assume that if the starting card exists in both the player's hand and on the player's side
			//of the field that it came from their hand UNLESS the field was full at the time of fusion.
			bool fusionStartsOnField = false;
			Card startingCard = OptimalPlay[0];

			if
			(
				!ValidHandCards.Any(handCard => handCard.CardId == startingCard.CardId) ||
				ValidFieldCards.Count() == 5
			)
			{
				fusionStartsOnField = true;
			}

			//Remove the starting card from either the hand cards or the field cards.
			int targetCardIndex;

			//Determine which card from the optimal sequence should be played to the field.
			//This is usually the last card in the sequence unless an equip card was applied.
			Card newFieldCard =
				OptimalPlay.Last().CardType == CardType.Monster ?
					OptimalPlay.Last() :
					OptimalPlay[OptimalPlay.Count - 2];

			if (fusionStartsOnField)
			{
				targetCardIndex = Array.IndexOf(_fieldCards, startingCard);

				//If the starting card came from the field, replace it with the fusion result.
				_fieldCards[targetCardIndex] = newFieldCard;
			}
			else
			{
				targetCardIndex = Array.IndexOf(_handCards, startingCard);

				//Otherwise, place the fusion result in the first empty slot 
				//on the field and clear the hand card that started the fusion.
				_handCards[targetCardIndex] = _deckList[0];

				int firstAvailableFieldSlotIndex = Array.IndexOf(_fieldCards, _deckList[0]);

				_fieldCards[firstAvailableFieldSlotIndex] = newFieldCard;
			}

			//Remove all cards involved in the fusion from the player's hand
			if (OptimalPlay.Count > 1)
			{
				for (int i = 1; i < OptimalPlay.Count - 1; i++)
				{
					targetCardIndex =
						Array.IndexOf(_handCards, OptimalPlay[i]);

					if (targetCardIndex != -1)
					{
						_handCards[targetCardIndex] = _deckList[0];
					}
				}
			}

			//If the last card in the sequence was not a monster, remove it as well
			if (OptimalPlay.Last().CardType != CardType.Monster)
			{
				targetCardIndex =
					Array.IndexOf(_handCards, OptimalPlay.Last());

				if (targetCardIndex != -1)
				{
					_handCards[targetCardIndex] = _deckList[0];
				}
			}

			//Condense the remaining hand cards down and backfill the rest with placeholders
			List<Card> newHand = ValidHandCards.ToList();
			int validHandCardCount = newHand.Count;

			if (validHandCardCount < 5)
			{
				for (int i = 0; i < 5 - validHandCardCount; i++)
				{
					newHand.Add(_deckList[0]);
				}
			}

			_handCards = newHand.ToArray();

			//Update the observable properties
			FieldCards = new ObservableCollection<Card>(_fieldCards);
			HandCards = new ObservableCollection<Card>(_handCards);

			ClearOptimalPlayData();

			RaisePropertyChanged(nameof(FieldCards));
			RaisePropertyChanged(nameof(HandCards));
		}


		public void ClearCardData()
		{
			SetPlaceholderCards();
			ClearOptimalPlayData();
		}
		#endregion



		#region Non-Public Method(s)
		private List<List<Card>> MapPotentialFusionPermutations()
		{
			List<List<Card>> potentialFusionPermutations = new List<List<Card>>();

			//Start by combining any non-placeholder monster cards in the player's
			//hand or on the player's side of the field as fusion 'root' cards.
			//NOTE: Field cards can ONLY serve as the root card for a fusion.
			List<Card> potentialFusionRoots =
				ValidFieldCards
				.Concat
				(
					ValidHandCards
				)
				.ToList();

			//Iterate over each potential fusion root card and 
			//generate a list of fusions that are possible for it 
			//based on the additional cards available to the player
			foreach (Card fusionRoot in potentialFusionRoots)
			{
				//Begin a permutation for each root card.  If all else fails, the
				//algorithm will pick a one-card permutation for the best root card.
				List<Card> currentPermutation = new List<Card>(new[] { fusionRoot });

				//Recursively generate potential fusions for each
				//root card and each of their potential permutations.
				potentialFusionPermutations.AddRange
				(
					GenerateFusionPermutations
					(
						currentPermutation,
						ValidHandCards.Except(new[] { fusionRoot })
					)
						.Distinct(new FusionPermutationEqualityComparer())
				);
			}

			return potentialFusionPermutations;
		}


		private List<List<Card>> GenerateFusionPermutations
		(
			List<Card> currentPermutation,
			IEnumerable<Card> potentialFusionMaterialCards
		)
		{
			List<List<Card>> generatedFusionPermutations = new List<List<Card>>();

			//Look at the last card of the current permutation being 
			//built to determine what card is the target of the fusion.
			Card targetCard = currentPermutation.Last();

			if (!potentialFusionMaterialCards.Any())
			{
				//If all potential fusion material cards have been exhausted,
				//add the current permutation to the list of those generated.
				generatedFusionPermutations.Add(currentPermutation);
			}
			else
			{

				//Iterate over each potential fusion material
				//card to resolve a potential fusion for it...
				foreach (Card potentialFusionMaterialCard in potentialFusionMaterialCards)
				{
					//Determine if either a general or specific fusions exists between  
					//the target card and the currently iterated fusion material card.
					Fusion resolvedFusion =
						ResolveResultantFusion(targetCard, potentialFusionMaterialCard);

					if (resolvedFusion != null)
					{
						//If a valid fusion was resolved, add the fusion material card and
						//resultant card to a new permutation copied from the current one.
						List<Card> branchPermutation = new List<Card>(currentPermutation);
						branchPermutation.Add(potentialFusionMaterialCard);
						branchPermutation.Add(resolvedFusion.ResultantCard);

						//Then recusively resolve permutations that can be made between the
						//current permutation and remaining potential fusion material cards.
						generatedFusionPermutations
							.AddRange
							(
								GenerateFusionPermutations
								(
									branchPermutation,
									potentialFusionMaterialCards.Except(new[] { potentialFusionMaterialCard })
								)
							);
					}
					else
					{
						//If no fusions were possible between the last card
						//of the permutation and remaining fusion material
						//cards, add the current permutation to the return list
						generatedFusionPermutations.Add(currentPermutation);
					}
				}
			}

			//Return the recursively build permutations generated for the current permutation
			return generatedFusionPermutations;
		}


		private Fusion ResolveResultantFusion
		(
			Card targetCard,
			Card fusionMaterialCard
		)
		{
			//A fusion between the two cards is possible if a 
			//fusion record exists where both the target and 
			//material cards possess an id or type matching
			//those of the respective cards being checked.
			IEnumerable<Fusion> possibleFusions =
				_fusionList
					.Where
					(
						fusion =>
							(
								(
									fusion.TargetCardId != null &&
									fusion.TargetCardId == targetCard.CardId
								) ||
								(
									fusion.TargetMonsterType != null &&
									(
										fusion.TargetMonsterType == targetCard.MonsterType ||
										fusion
											.TargetMonsterType
											.In
											(
												targetCard
													.SecondaryTypes?
													.Select
													(
														secondaryType =>
															(MonsterType?)secondaryType.MonsterType
													) ??
														new[] { targetCard.MonsterType }
											)
									)
								)
							) &&
							(
								(
									fusion.FusionMaterialCardId != null &&
									fusion.FusionMaterialCardId == fusionMaterialCard.CardId
								) ||
								(
									fusion.FusionMaterialMonsterType != null &&
									(
										fusion.FusionMaterialMonsterType == fusionMaterialCard.MonsterType ||
										fusion
											.FusionMaterialMonsterType
											.In
											(
												fusionMaterialCard
													.SecondaryTypes?
													.Select
													(
														secondaryType =>
															(MonsterType?)secondaryType.MonsterType
													) ??
														new[] { fusionMaterialCard.MonsterType }
											)
									)
								)
							)
					);

			//If a specific fusion exists between the two cards, return it.
			if(possibleFusions.Any(fusion => fusion.FusionType == FusionType.Specific))
			{
				return
					possibleFusions
						.First(fusion => fusion.FusionType == FusionType.Specific);
			}

			//If no specific fusions exist, take the general fusion with the lowest
			//attack greater than those of the cards used to form the fusion.
			int maxFusionMaterialAttackPoints =
				Math.Max
				(
					targetCard.AttackPoints ?? 0,
					fusionMaterialCard.AttackPoints ?? 0
				);

			return
				possibleFusions
					.Where
					(
						fusion =>
							fusion.FusionType == FusionType.General &&
							fusion.ResultantCard.AttackPoints > maxFusionMaterialAttackPoints
					)
					.OrderBy
					(
						fusion =>
							fusion.ResultantCard.AttackPoints ?? 0
					)
					.FirstOrDefault();
		}


		private List<Card> DetermineOptimalFusionPermutation
		(
			List<List<Card>> potentialFusionPermutations,
			List<Equippable> potentialEquips,
			List<Terrain> potentialTerrains
		)
		{
			//Determine which permutation to use as the initial optimal permutation.
			List<Card> initialPermutation =
				DetermineInitialOptimalPermutation
				(
					potentialFusionPermutations,
					potentialEquips,
					potentialTerrains
				);

			List<Card> optimalFusionPermutation = new List<Card>();

			if (ValidFieldCards.Count() < 5)
			{
				//If the field is NOT full, order the list of permutations by the strength of the resultant 
				//card (in descending order).  In the event that multiple permutations result in the same
				//fusion result, further order the permutations by the combined strength of the fusion  
				//material cards (ascending) to derive the best fusion for the least amount of sacrifice.
				optimalFusionPermutation = initialPermutation;
				ThrowAwayFirstCardInSequence = false;
			}
			else if
			(
				potentialFusionPermutations
					.Any
					(
						permutation =>
							permutation.First().AttackPoints < 
								CalculateModifiedAttack
								(
									permutation.Last(),
									potentialEquips,
									potentialTerrains
								)
					)
			)
			{
				//If the field IS full, a monster on the field must be sacrificed.  In this 
				//scenario, first attempt to filter the permutations to those where the resultant 
				//card is stronger than the monster on the field which is being sacrificed.
				//If any permutations meeting this criteria exist, take the resultant card with
				//the highest attack that is formed by sacrificing the monster with the lowest attack.
				optimalFusionPermutation =
					potentialFusionPermutations
						.Where
						(
							permutation =>
								permutation.First().AttackPoints <
									CalculateModifiedAttack
									(
										permutation.Last(),
										potentialEquips,
										potentialTerrains
									)
						)
						.OrderByDescending
						(
							permutation =>
								CalculateModifiedAttack
								(
									permutation.Last(),
									potentialEquips,
									potentialTerrains
								)
						)
						.ThenBy
						(
							permutation =>
								permutation.First().AttackPoints
						)
						.ThenBy
						(
							//If any further sorting is necessary (due to multiple 
							//permutations using the same field card with the same
							//resultant card), use the permutation with the fewest cards
							permutation =>
								permutation.Count
						)
						.FirstOrDefault();

				//Update the starting card of the permutation to
				//reflect the field monster being sacrificed.
				optimalFusionPermutation =
					UpdateStartingCardForPermutation
					(
						optimalFusionPermutation,
						potentialFusionPermutations
					);
			}
			else
			{
				//If all else fails, take the fusion permutation that involves sacrificing the
				//field monster with the lowest attack for the fusion result with the highest.
				optimalFusionPermutation = initialPermutation;

				//Update the starting card of the permutation to
				//reflect the field monster being sacrificed.
				optimalFusionPermutation =
					UpdateStartingCardForPermutation
					(
						optimalFusionPermutation,
						potentialFusionPermutations
					);
			}

			return optimalFusionPermutation;
		}


		private List<Card> DetermineInitialOptimalPermutation
		(
			List<List<Card>> potentialFusionPermutations,
			List<Equippable> potentialEquips,
			List<Terrain> potentialTerrains
		)
		{
			return
				potentialFusionPermutations
					.Where
					(
						//Exclude single-card permutations (indicating no possible fusions)
						//unless the strongest possible card is in the player's hand, or an
						//equip card or field card in the player's hand can be used on it.
						permutation =>
							permutation.Count > 1 ||
							permutation[0].In(_handCards) ||
							potentialEquips
								.Any
								(
									equippable =>
										equippable.TargetCardId == permutation.Last().CardId
								) ||
							potentialTerrains
								.Any
								(
									terrain =>
										terrain.In(permutation.Last().FavorableTerrains())
								)
					)
					.OrderByDescending
					(
						//Order the possible permutations by the attack points of the
						//resultant card (descending), also factoring in equip cards
						permutation =>
							CalculateModifiedAttack
							(
								permutation.Last(),
								potentialEquips,
								potentialTerrains
							)
					)
					.ThenBy
					(
						permutation =>
							permutation.Sum(card => card.AttackPoints)
					)
					.FirstOrDefault();
		}


		private string ModifyOptimalPlayText()
		{
			Card resultantCard = OptimalPlay?.LastOrDefault();

			if
			(
				(
					(_availableEquipCards?.Any() ?? false) ||
					(_availableFieldCards?.Any() ?? false)
				) &&
				resultantCard != null
			)
			{

				(int attack, int defense) modifiedStats =
					(
						resultantCard.AttackPoints ?? 0,
						resultantCard.DefensePoints ?? 0
					);

				return
					(_availableEquipCards ?? new List<Card>())
						.Concat(_availableFieldCards ?? new List<Card>())
						.Aggregate
						(
							modifiedStats,
							(currentStats, equipCard) =>
							{
								if (equipCard.CardId == 657)
								{
									modifiedStats.attack += 1000;
									modifiedStats.defense += 1000;
								}
								else
								{
									modifiedStats.attack += 500;
									modifiedStats.defense += 500;
								}

								return modifiedStats;
							},
							modifiedStats =>
								$"({resultantCard.CardId}) {resultantCard.Name} \n" +
								$"[ATK: {modifiedStats.attack} | DEF: {modifiedStats.defense} ]"
						);
			}
			else if (resultantCard != null)
			{
				return resultantCard.ToString();
			}
			else
			{
				return string.Empty;
			}
		}


		private int CalculateModifiedAttack
		(
			Card targetCard, 
			IEnumerable<Equippable> potentialEquips,
			IEnumerable<Terrain> potentialTerrains
		)
		{
			int calculatedAttack = targetCard.AttackPoints ?? 0;

			foreach
			(
				Equippable equippable in 
				potentialEquips.Where(equip => equip.TargetCard.Equals(targetCard))
			)
			{
				if (equippable.EquipCardId == 657)
				{
					//If the equip card is 'Megamorph', add 1000 to the calculated attack
					calculatedAttack += 1000;
				}
				else
				{
					//For all other equip cards, add 500 to the calculated attack
					calculatedAttack += 500;
				}
			}

			if(potentialTerrains?.Any(terrain => terrain.In(targetCard.FavorableTerrains())) ?? false)
			{
				//If any terrains in the player's hand would benefit
				//the target card, add 500 to the calculated attack.
				calculatedAttack += 500;
			}

			return calculatedAttack;
		}


		private List<Card> UpdateStartingCardForPermutation
		(
			List<Card> targetPermutation,
			List<List<Card>> potentialPermutations
		)
		{
			if (!targetPermutation[0].In(_fieldCards))
			{
				//If the optimal fusion does not start with a card on the field, prepend the weakest card
				//currently on the field that won't fuse with (and throw off) the first card of the sequence.
				List<Card> weakestFieldCards =
					_fieldCards
						.OrderBy
						(
							fieldCard =>
								fieldCard.AttackPoints
						)
						.ToList();

				Card sacrificeCard =
					weakestFieldCards
						.FirstOrDefault
						(
							fieldCard =>
								!potentialPermutations
									.Any
									(
										potentialPermutation =>
											potentialPermutation.Count > 1 &&
											potentialPermutation[0].CardId == fieldCard.CardId &&
											potentialPermutation[1].CardId == targetPermutation[0].CardId
									)
						);

				//If all cards on the field can fuse with the starting card of the 
				//optimal sequence, default to the overall weakest card on the field
				targetPermutation =
					targetPermutation
						.Prepend
						(
							sacrificeCard ?? weakestFieldCards.First()
						)
						.ToList();

				ThrowAwayFirstCardInSequence = sacrificeCard != null;
			}
			else
			{
				ThrowAwayFirstCardInSequence = false;
			}

			return targetPermutation;
		}


		private List<Card> BuildOptimalPlaySequence(List<Card> optimalFusionPermutation)
		{
			List<Card> optimalFusionSequence = new List<Card>();

			//Add the root card of the permutation to the fusion sequence to be displayed.
			optimalFusionSequence.Add(optimalFusionPermutation[0]);

			if (optimalFusionPermutation.Count > 1)
			{
				if (ThrowAwayFirstCardInSequence)
				{
					//If the first card in the sequence is a throw-away card, add
					//the second card of the permutation to the sequence as well
					optimalFusionSequence.Add(optimalFusionPermutation[1]);
				}

				//If the permutation involves more than one card, take every other card in the sequence
				//(which corresponds to the fusion material cards of which the resultant card is composed)
				for (int i = optimalFusionSequence.Count; i < optimalFusionPermutation.Count; i += 2)
				{
					//Add each to the fusion sequence to be displayed
					optimalFusionSequence.Add(optimalFusionPermutation[i]);
				}

				//Add the last card of the permutation to the 
				//sequence (representing the final resultant card).
				optimalFusionSequence.Add(optimalFusionPermutation.Last());
			}

			return optimalFusionSequence;
		}


		private void ClearOptimalPlayData()
		{
			OptimalPlay?.Clear();
			_availableEquipCards?.Clear();
			_availableFieldCards?.Clear();
			ThrowAwayFirstCardInSequence = false;

			RaisePropertyChanged(nameof(HandCardFieldFlags));
			RaisePropertyChanged(nameof(FieldCardAvailable));
			RaisePropertyChanged(nameof(HandCardEquipmentFlags));
			RaisePropertyChanged(nameof(EquipCardAvailable));
			RaisePropertyChanged(nameof(ModifierCardAvailable));
			RaisePropertyChanged(nameof(OptimalPlayEnhancedText));
			RaisePropertyChanged(nameof(OptimalPlay));
			RaisePropertyChanged(nameof(OptimalPlayCardCount));
			RaisePropertyChanged(nameof(GenerateFusionEnabled));
			RaisePropertyChanged(nameof(AcceptFusionEnabled));
			RaisePropertyChanged(nameof(ClearCardDataEnabled));
			RaisePropertyChanged(nameof(ThrowAwayFirstCardInSequence));
		}


		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?
				.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
