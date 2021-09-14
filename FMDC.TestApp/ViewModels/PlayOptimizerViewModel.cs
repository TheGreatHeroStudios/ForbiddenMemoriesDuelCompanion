using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TGH.Common.Extensions;
using TGH.Common.Repository.Interfaces;

namespace FMDC.TestApp.ViewModels
{
	public class PlayOptimizerViewModel : INotifyPropertyChanged
	{
		#region Non-Public Member(s)
		private IGenericRepository _cardRepository;
		private List<Card> _cardList;
		private List<Fusion> _fusionList;
		private List<Equippable> _equippableList;

		private Card[] _handCards = new Card[5];
		private Card[] _fieldCards = new Card[5];
		#endregion



		#region Public Propert(ies)
		public List<Card> CardList => _cardList;
		public ObservableCollection<Card> HandCards { get; set; }
		public ObservableCollection<Card> FieldCards { get; set; }
		public ObservableCollection<Card> OptimalPlay { get; set; }
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
		public PlayOptimizerViewModel(IGenericRepository cardRepository)
		{
			_cardRepository = cardRepository;

			LoadCardData();
			SetPlaceholderCards();
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Public Method(s)
		public void SetPlaceholderCards()
		{
			for (int i = 0; i < 5; i++)
			{
				_handCards[i] = _cardList[0];
				_fieldCards[i] = _cardList[0];
			}

			FieldCards = new ObservableCollection<Card>(_fieldCards);
			HandCards = new ObservableCollection<Card>(_handCards);

			RaisePropertyChanged(nameof(FieldCards));
			RaisePropertyChanged(nameof(HandCards));
			RaisePropertyChanged(nameof(GenerateFusionEnabled));
			RaisePropertyChanged(nameof(ClearCardDataEnabled));
		}


		public void UpdateCardSelection(Card updatedCard, bool handCardUpdated, int index)
		{
			if (handCardUpdated)
			{
				_handCards[index] = updatedCard;
				HandCards = new ObservableCollection<Card>(_handCards);

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
		}


		public void DetermineOptimalPlay()
		{
			//Get a list of all potential fusion permutations available
			//based on the cards in the player's hand and on the field.
			List<List<Card>> potentialFusionPermutations = MapPotentialFusionPermutations();

			if (potentialFusionPermutations.Any())
			{
				//Get the optimal fusion permutation to use
				List<Card> optimalFusionPermutation =
					DetermineOptimalFusionPermutation(potentialFusionPermutations);

				List<Card> optimalPlay =
					BuildOptimalPlaySequence(optimalFusionPermutation);

				Func<Equippable, bool> equipPredicate =
					equippable =>
						equippable.EquipCard.In(_handCards) &&
						equippable.TargetCard.CardId == optimalFusionPermutation.Last().CardId;

				//If the player has any applicable equip cards in their hand, append the 
				//first one to the optimal play sequence to enhance the card's strength.
				if (_equippableList.Any(equipPredicate))
				{
					optimalPlay
						.Add(_equippableList.First(equipPredicate).EquipCard);
				}

				//Finally, assign the sequence to the observable collection for displaying in the UI
				OptimalPlay = new ObservableCollection<Card>(optimalPlay);

				RaisePropertyChanged(nameof(OptimalPlay));
				RaisePropertyChanged(nameof(OptimalPlayCardCount));
				RaisePropertyChanged(nameof(AcceptFusionEnabled));
				RaisePropertyChanged(nameof(ThrowAwayFirstCardInSequence));
			}
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
				_handCards[targetCardIndex] = _cardList[0];

				int firstAvailableFieldSlotIndex = Array.IndexOf(_fieldCards, _cardList[0]);

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
						_handCards[targetCardIndex] = _cardList[0];
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
					_handCards[targetCardIndex] = _cardList[0];
				}
			}

			//Condense the remaining hand cards down and backfill the rest with placeholders
			List<Card> newHand = ValidHandCards.ToList();
			int validHandCardCount = newHand.Count;

			if (validHandCardCount < 5)
			{
				for (int i = 0; i < 5 - validHandCardCount; i++)
				{
					newHand.Add(_cardList[0]);
				}
			}

			_handCards = newHand.ToArray();

			//Clear optimal fusion data
			OptimalPlay.Clear();

			//Update the observable properties
			FieldCards = new ObservableCollection<Card>(_fieldCards);
			HandCards = new ObservableCollection<Card>(_handCards);

			ClearOptimalFusionData();

			RaisePropertyChanged(nameof(FieldCards));
			RaisePropertyChanged(nameof(HandCards));
		}


		public void ClearCardData()
		{
			SetPlaceholderCards();
			ClearOptimalFusionData();
		}
		#endregion



		#region Non-Public Method(s)
		private void LoadCardData()
		{
			//Retrieve card images and secondary types
			//which will be associated to the card list
			IEnumerable<GameImage> cardThumbnails =
				_cardRepository
					.RetrieveEntities<GameImage>
					(
						gameImage =>
							gameImage.EntityType == ImageEntityType.Card
					);

			IEnumerable<SecondaryType> secondaryTypes =
				_cardRepository
					.RetrieveEntities<SecondaryType>
					(
						secondaryType => true
					);

			//Initialize the card list and add a placeholder for no selection.
			_cardList = new List<Card>();
			_cardList.Add
			(
				new Card
				{
					CardId = -1,
					Name = "None"
				}
			);

			//Join the card images and secondary types to their appropriate card
			//and use the retrieved list of cards to hydrate the in-memory collection 
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
					.LeftJoin
					(
						secondaryTypes
							.GroupBy
							(
								secondaryType =>
									secondaryType.CardId
							),
						card => card.CardId,
						secondaryTypes => secondaryTypes.Key,
						(card, secondaryTypeList) =>
						{
							card.SecondaryTypes = secondaryTypeList?.ToList();
							return card;
						}
					)
					.OrderBy(card => card.Name)
					.ToList()
			);

			//Load the list of available fusions and associate each fusion with its resultant card
			_fusionList =
				_cardRepository
					.RetrieveEntities<Fusion>(fusion => true)
					.Join
					(
						_cardList,
						fusion => fusion.ResultantCardId,
						resultantCard => resultantCard.CardId,
						(fusion, resultantCard) =>
						{
							fusion.ResultantCard = resultantCard;
							return fusion;
						}
					)
					.ToList();

			//Load the list of equipment and its associated cards
			_equippableList =
				_cardRepository
					.RetrieveEntities<Equippable>(equippable => true)
					.Join
					(
						_cardList,
						equippable => equippable.EquipCardId,
						equipCard => equipCard.CardId,
						(equippable, equipCard) =>
						{
							equippable.EquipCard = equipCard;
							return equippable;
						}
					)
					.Join
					(
						_cardList,
						equippable => equippable.TargetCardId,
						targetCard => targetCard.CardId,
						(equippable, targetCard) =>
						{
							equippable.TargetCard = targetCard;
							return equippable;
						}
					)
					.ToList();
		}


		private List<List<Card>> MapPotentialFusionPermutations()
		{
			List<List<Card>> potentialFusionPermutations = new List<List<Card>>();

			//Start by combining any non-placeholder cards in the player's
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
				//card to generate potential fusions for it...
				foreach (Card potentialFusionMaterialCard in potentialFusionMaterialCards)
				{
					//Determine if either general or specific fusions exist between the 
					//target card and the currently iterated potential fusion material
					List<Fusion> potentialFusions =
						RetrieveViableFusions(targetCard, potentialFusionMaterialCard).ToList();

					if (potentialFusions.Any())
					{
						//If at least one potential fusion exists, branch the starting permutation
						//into seperate permutations for each potential fusion that can be executed.
						IEnumerable<List<Card>> branchedPermutations =
							potentialFusions
								.Select
								(
									potentialFusion =>
									{
										List<Card> branchedPermutation =
											new List<Card>(currentPermutation);

										//Add the fusion material card and resultant
										//card to the current permutation (in that order)
										branchedPermutation.Add(potentialFusionMaterialCard);
										branchedPermutation.Add(potentialFusion.ResultantCard);

										return branchedPermutation;
									}
								);

						//Then, iterate over each branched permutation and recursively build 
						//upon it by generating additional permutations from remaining cards.
						foreach (List<Card> branchedPermutation in branchedPermutations)
						{
							generatedFusionPermutations
								.AddRange
								(
									GenerateFusionPermutations
									(
										branchedPermutation,
										potentialFusionMaterialCards.Except(new[] { potentialFusionMaterialCard })
									)
								);
						}
					}
					else
					{
						//When generating a permutation that is not for a branched fusion, if no
						//potential fusions exist between the permutation target and the current fusion 
						//material card, add the current permutation to this point to the return list.
						generatedFusionPermutations.Add(currentPermutation);
					}
				}
			}

			//Return the recursively build permutations generated for the current permutation
			return generatedFusionPermutations;
		}


		private IEnumerable<Fusion> RetrieveViableFusions
		(
			Card targetCard,
			Card fusionMaterialCard
		)
		{
			//A fusion between the two cards is possible if a fusion record exists where both the target and
			//fusion material cards possess an id or type matching those of the respective cards being checked.
			IEnumerable<Fusion> possibleFusions =
				_fusionList
					.Where
					(

						fusion =>
							(


									fusion.TargetCardId != null &&
									fusion.TargetCardId == targetCard.CardId
								 ||

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

							) &&
							(

									fusion.FusionMaterialCardId != null &&
									fusion.FusionMaterialCardId == fusionMaterialCard.CardId
								 ||

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
					);

			//In terms of which fusions are actually viable (out of those that are possible), take
			//the specific fusion between the two cards (if one exists) and take the general fusion
			//whose resultant card has the LOWEST attack greater than those of its material cards.
			int maxFusionMaterialAttackPoints =
				Math.Max
				(
					targetCard.AttackPoints ?? 0,
					fusionMaterialCard.AttackPoints ?? 0
				);

			IEnumerable<Fusion> viableFusions =
				possibleFusions
					.Where
					(
						fusion =>
							fusion.FusionType == FusionType.Specific
					)
					.Concat
					(
						new[]
						{
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
								.FirstOrDefault()
						}
					)
					.Where
					(
						fusion =>
							fusion != null
					);

			return viableFusions;
		}


		private List<Card> DetermineOptimalFusionPermutation
		(
			List<List<Card>> potentialFusionPermutations
		)
		{
			//Determine which permutation to use as the initial optimal permutation.
			List<Card> initialPermutation =
				DetermineInitialPermutationToUse(potentialFusionPermutations);

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
							permutation.First().AttackPoints < permutation.Last().AttackPoints
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
								permutation.First().AttackPoints < permutation.Last().AttackPoints
						)
						.OrderByDescending
						(
							permutation =>
								permutation.Last().AttackPoints
						)
						.ThenBy
						(
							permutation =>
								permutation.First().AttackPoints
						)
						.ThenBy
						(
							permutation =>
								permutation.Sum(card => card.AttackPoints)
						)
						.FirstOrDefault();

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

				optimalFusionPermutation =
					UpdateStartingCardForPermutation
					(
						optimalFusionPermutation,
						potentialFusionPermutations
					);
			}

			return optimalFusionPermutation;
		}


		private List<Card> DetermineInitialPermutationToUse
		(
			List<List<Card>> potentialFusionPermutations
		)
		{
			return
				potentialFusionPermutations
						.Where
						(
							//Exclude single-card permutations (indicating no possible fusions)
							//unless the strongest possible card is in the player's hand.
							permutation =>
								permutation.Count > 1 ||
								permutation[0].CardId
									.In
									(
										_handCards
											.Select
											(
												handCard =>
													handCard.CardId
											)
									)
						)
						.OrderByDescending
						(
							permutation =>
								permutation.Last().AttackPoints
						)
						.ThenBy
						(
							permutation =>
								permutation.Sum(card => card.AttackPoints)
						)
						.FirstOrDefault();
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


		private void ClearOptimalFusionData()
		{
			OptimalPlay?.Clear();
			ThrowAwayFirstCardInSequence = false;

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
