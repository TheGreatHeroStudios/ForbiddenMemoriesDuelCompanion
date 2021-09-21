using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
		#endregion
	}
}
