using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TGH.Common.DataStructures;
using TGH.Common.Extensions;
using TGH.Common.Repository.Interfaces;

namespace FMDC.BusinessLogic
{
	public class FusionService : IFusionService
	{
		#region Non-Public Member(s)
		private List<Fusion> _fusionList;
		#endregion



		#region Constructor(s)
		public FusionService(IGenericRepository cardRepository)
		{
			List<Card> cardList =
				cardRepository
					.RetrieveEntities<Card>(card => true)
					.ToList();

			_fusionList =
				cardRepository
					.RetrieveEntities<Fusion>(fusion => true)
					.Join
					(
						cardList,
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


		public FusionService(List<Fusion> fusions)
		{
			_fusionList = fusions;
		}
		#endregion



		#region 'IFusionService' Implementation
		public void BuildFusionTree
		(
			BalancedBinaryTree<Card> currentTree,
			List<Fusion> viableFusions,
			Dictionary<Card, int> availableCardCounts
		)
		{
			List<Card> requiredRootCards =
				currentTree
					.Where
					(
						node =>

					)
			if()
		}


		public List<Card> ConvertToPlaySequence
		(
			List<Card> fusionPermutation,
			bool includeResultantCard = true,
			bool throwAwayFirstCardInSequence = false
		)
		{
			List<Card> playSequence = new List<Card>();

			//Add the root card of the permutation to the play sequence to be displayed.
			playSequence.Add(fusionPermutation[0]);

			if (fusionPermutation.Count > 1)
			{
				if (throwAwayFirstCardInSequence)
				{
					//If the first card in the sequence is a throw-away card, add
					//the second card of the permutation to the sequence as well
					playSequence.Add(fusionPermutation[1]);
				}

				//If the permutation involves more than one card, take every other card in the sequence
				//(which corresponds to the fusion material cards of which the resultant card is composed)
				for (int i = playSequence.Count; i < fusionPermutation.Count; i += 2)
				{
					//Add each to the play sequence to be displayed
					playSequence.Add(fusionPermutation[i]);
				}

				if (includeResultantCard)
				{
					//Add the last card of the permutation to the 
					//sequence (representing the final resultant card).
					playSequence.Add(fusionPermutation.Last());
				}
			}

			return playSequence;
		}


		public List<Fusion> DetermineViableSpecificFusions
		(
			Dictionary<Card, int> actualCardCounts
		)
		{
			List<Fusion> viableFusions = new List<Fusion>();

			//Build a preliminary dictionary of card counts representing the cards 
			//the player actually owns and initialize it with the provided card counts
			//(to prevent modifying the card counts from the player's deck and trunk).
			Dictionary<Card, int> availableCardCounts = new Dictionary<Card, int>(actualCardCounts);

			List<Fusion> availableFusions = 
				new List<Fusion>
				(
					_fusionList
						.Where
						(
							fusion =>
								fusion.FusionType == FusionType.Specific
						)
				);

			//Recursively determine which fusions can be made given a current set of available cards.
			DetermineViableSpecificFusions
			(
				availableFusions, 
				availableCardCounts,
				viableFusions
			);

			return viableFusions;
		}


		public List<List<Card>> GenerateFusionPermutations
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
		#endregion



		#region Non-Public Method(s)
		private bool CheckFusionTypeCompatibility(MonsterType? fusionType, Card card)
		{
			return
				fusionType == card.MonsterType ||
				fusionType
					.In
					(
						card
							.SecondaryTypes?
							.Select
							(
								secondaryType =>
									(MonsterType?)secondaryType.MonsterType
							) ??
								new[] { card.MonsterType }
					);
		}


		private void DetermineViableSpecificFusions
		(
			List<Fusion> availableFusions,
			Dictionary<Card, int> availableCardCounts,
			List<Fusion> viableFusions
		)
		{
			//Filter the current set of available fusions to only include
			//those which can be made with the available card counts
			List<Fusion> viableFusionsThisIteration =
				availableFusions
					.Where
					(
						//A fusion is viable if the player owns (or has fused) at 
						//least one of each material card necessary for it (or two 
						//if the fusion's material cards are both the same card)
						availableFusion =>
							availableCardCounts.ContainsKey(availableFusion.TargetCard) &&
							availableCardCounts.ContainsKey(availableFusion.FusionMaterialCard) &&
							(
								(
									availableFusion
										.TargetCard
										.Equals(availableFusion.FusionMaterialCard) &&
									availableCardCounts[availableFusion.TargetCard] >= 2
								) ||
								(
									availableCardCounts[availableFusion.TargetCard] >= 1 &&
									availableCardCounts[availableFusion.FusionMaterialCard] >= 1
								)
							)
					)
					.ToList();

			if(viableFusionsThisIteration.Any())
			{
				//If any of the available fusions are viable this 
				//iteration, first add them to the output collection.
				viableFusions.AddRange(viableFusionsThisIteration);

				//Build a new list of available fusions which includes ALL specific 
				//fusions except those which have already been determined to be viable.
				availableFusions =
					new List<Fusion>
					(
						_fusionList
							.Where
							(
								fusion =>
									fusion.FusionType == FusionType.Specific
							)
							.Except
							(
								viableFusions
							)
					);

				//Cache the available card counts this iteration to reset after
				//each recursive call (which modifies the card counts)
				Dictionary<Card, int> availableCardCountsThisIteration =
					new Dictionary<Card, int>(availableCardCounts);

				//Then, iterate over each one and recusively determine new viable 
				//fusions based on the modified card counts (subtracting the cards 
				//that were used to make the fusion, and adding the resultant card)
				foreach (Fusion viableFusion in viableFusionsThisIteration)
				{
					//Remove card counts for the fusion material cards
					if(viableFusion.TargetCard.Equals(viableFusion.FusionMaterialCard))
					{
						availableCardCounts[viableFusion.TargetCard] =
							availableCardCounts[viableFusion.TargetCard] - 2;
					}
					else
					{
						availableCardCounts[viableFusion.TargetCard] =
							availableCardCounts[viableFusion.TargetCard] - 1;

						availableCardCounts[viableFusion.FusionMaterialCard] =
							availableCardCounts[viableFusion.FusionMaterialCard] - 1;
					}

					//Add card count for the resultant card
					if (!availableCardCounts.ContainsKey(viableFusion.ResultantCard))
					{
						availableCardCounts.Add(viableFusion.ResultantCard, 1);
					}
					else
					{
						availableCardCounts[viableFusion.ResultantCard] =
							availableCardCounts[viableFusion.ResultantCard] + 1;
					}

					//Call the current method recursively to continue building viable fusions
					DetermineViableSpecificFusions
					(
						availableFusions,
						availableCardCounts,
						viableFusions
					);

					//After recursively determining viable fusions, reset the card
					//count to what it was at the beginning of the current iteration.
					availableCardCounts = 
						new Dictionary<Card, int>(availableCardCountsThisIteration);
				}
			}
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
									fusion.TargetCard.Equals(targetCard)
								) ||
								CheckFusionTypeCompatibility
								(
									fusion.TargetMonsterType, 
									targetCard
								)
							) &&
							(
								(
									fusion.FusionMaterialCard != null &&
									fusion.FusionMaterialCard.Equals(fusionMaterialCard)
								) ||
								CheckFusionTypeCompatibility
								(
									fusion.FusionMaterialMonsterType, 
									fusionMaterialCard
								)
							)
					);

			//If a specific fusion exists between the two cards, return it.
			if (possibleFusions.Any(fusion => fusion.FusionType == FusionType.Specific))
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
		#endregion
	}
}
