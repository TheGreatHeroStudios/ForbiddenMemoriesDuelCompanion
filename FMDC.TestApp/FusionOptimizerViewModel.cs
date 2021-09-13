using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TGH.Common.Extensions;
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
		public ObservableCollection<Card> HandCards { get; set; }
		public ObservableCollection<Card> FieldCards { get; set; }
		public ObservableCollection<Card> OptimalFusionSequence { get; set; }
		public int OptimalFusionCardCount => OptimalFusionSequence?.Count ?? 0;

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
		#endregion



		#region Constructor(s)
		public FusionOptimizerViewModel(IGenericRepository cardRepository)
		{
			_cardRepository = cardRepository;

			LoadCardData();
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
		}


		public void UpdateCardSelection(Card updatedCard, bool handCardUpdated, int index)
		{
			if (handCardUpdated)
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


		public void GenerateOptimalFusion()
		{
			//Get a list of all potential fusion permutations available
			//based on the cards in the player's hand and on the field.
			List<List<Card>> potentialFusionPermutations = MapPotentialFusionPermutations();

			if (potentialFusionPermutations.Any())
			{
				List<Card> optimalFusionSequence = new List<Card>();

				//Order the list of permutations by the strength of the resultant  
				//card (in descending order).  In the event that multiple permutations  
				//result in the same fusion result, further order the permutations by 
				//the combined strength of the fusion material cards (ascending) 
				//to derive the best fusion for the least amount of sacrifice.
				List<Card> optimalFusionPermutation =
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
						.First();

				//Add the root card of the permutation to the fusion sequence to be displayed.
				optimalFusionSequence.Add(optimalFusionPermutation[0]);

				if (optimalFusionPermutation.Count > 1)
				{
					//If the permutation involves more than one card, take every other card in the sequence
					//(which corresponds to the fusion material cards of which the resultant card is composed)
					for (int i = 1; i < optimalFusionPermutation.Count; i += 2)
					{
						//Add each to the fusion sequence to be displayed
						optimalFusionSequence.Add(optimalFusionPermutation[i]);
					}

					//Add the last card of the permutation to the 
					//sequence (representing the final resultant card).
					optimalFusionSequence.Add(optimalFusionPermutation.Last());
				}

				//Finally, assign the sequence to the observable collection for displaying in the UI
				OptimalFusionSequence = new ObservableCollection<Card>(optimalFusionSequence);

				RaisePropertyChanged(nameof(OptimalFusionSequence));
				RaisePropertyChanged(nameof(OptimalFusionCardCount));
			}
		}


		public void AcceptFusion()
		{
			//Determine whether the accepted fusion started with a card from the hand or from the field

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
		}


		private List<List<Card>> MapPotentialFusionPermutations()
		{
			List<List<Card>> potentialFusionPermutations = new List<List<Card>>();

			//Start by combining any non-placeholder cards in the player's
			//hand or on the player's side of the field as fusion 'root' cards.


			//NOTE: Field cards can ONLY serve as the root card for a fusion
			//and if the field is full, fusions MUST start with a field card.
			List<Card> potentialFusionRoots = ValidFieldCards.ToList();

			if (potentialFusionRoots.Count < 5)
			{
				potentialFusionRoots.AddRange(ValidHandCards);
			}

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


		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?
				.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
