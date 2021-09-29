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
		private List<Fusion> _generalFusions;
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

			_generalFusions =
				_fusionList
					.Where
					(
						fusion =>
							fusion.FusionType == FusionType.General
					)
					.ToList();
		}


		public FusionService(List<Fusion> fusions)
		{
			_fusionList = fusions;

			_generalFusions =
				_fusionList
					.Where
					(
						fusion =>
							fusion.FusionType == FusionType.General
					)
					.ToList();
		}
		#endregion



		#region 'IFusionService' Implementation
		public bool BuildFusionTree
		(
			BinaryTreeNode<Card> fusionLeafNode,
			List<Fusion> viableFusions,
			Dictionary<Card, int> availableCardCounts
		)
		{
			bool fusionPossible = true;

			//Get the best fusion for the leaf node card by selecting the fusion
			//from the list of viable fusions resulting in the leaf node card
			//which has the highest average attack across its material monsters.
			Fusion optimalFusion =
				viableFusions
					.Where
					(
						fusion =>
							fusion.ResultantCard.Equals(fusionLeafNode.Data)
					)
					.OrderByDescending
					(
						fusion =>
							(float)
								(
									fusion.TargetCard.AttackPoints +
									fusion.FusionMaterialCard.AttackPoints
								) / 2
					)
					.FirstOrDefault();

			if (optimalFusion != null)
			{
				//If a viable fusion was selected for the leaf node,
				//remove the selected fusion from the list of viables.
				viableFusions.Remove(optimalFusion);

				//Add new leaf nodes for each of the material cards
				//necessary to form the current leaf node.
				BinaryTreeNode<Card> fusionTreeLeftChildNode =
					new BinaryTreeNode<Card>(optimalFusion.TargetCard);

				fusionLeafNode.LeftChildNode = fusionTreeLeftChildNode;

				BinaryTreeNode<Card> fusionTreeRightChildNode =
					new BinaryTreeNode<Card>(optimalFusion.FusionMaterialCard);

				fusionLeafNode.RightChildNode = fusionTreeRightChildNode;

				if
				(
					!availableCardCounts.ContainsKey(optimalFusion.TargetCard) ||
					availableCardCounts[optimalFusion.TargetCard] < 1
				)
				{
					//If the player does not have the card needed to make the target
					//card's child node added to the tree, recursively build additional
					//leaf nodes from the fusion material cards until the player 
					//has the cards necessary to form the entire fusion tree.
					fusionPossible =
						BuildFusionTree
						(
							fusionTreeLeftChildNode,
							viableFusions,
							availableCardCounts
						);
				}
				else
				{
					//If the player has the necessary card, subtract one from its
					//available card count to indicate that the card has been used.
					availableCardCounts[optimalFusion.TargetCard] =
						availableCardCounts[optimalFusion.TargetCard] - 1;
				}

				//Perform the same process for the fusion material card's child node.
				if
				(
					!availableCardCounts.ContainsKey(optimalFusion.FusionMaterialCard) ||
					availableCardCounts[optimalFusion.FusionMaterialCard] < 1
				)
				{
					fusionPossible =
						BuildFusionTree
						(
							fusionTreeRightChildNode,
							viableFusions,
							availableCardCounts
						);
				}
				else
				{
					availableCardCounts[optimalFusion.FusionMaterialCard] =
						availableCardCounts[optimalFusion.FusionMaterialCard] - 1;
				}
			}
			else
			{
				//If no viable fusion was selected (most likely due to it
				//being utilized by another optimization) return 'false'
				//indicating that the fusion is no longer possible.
				fusionPossible = false;
			}

			return fusionPossible;
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


		public List<Card> GetGeneralFusionCards
		(
			Card targetCard,
			Dictionary<Card, int> availableCardCounts
		)
		{
			List<Card> generalFusionCards = new List<Card>();

			if
			(
				_generalFusions
					.Any
					(
						fusion => 
							fusion.ResultantCard.Equals(targetCard)
					)
			)
			{
				//If any general fusions exist for the target card,
				//iterate over each and determine if the player has
				//the cards to create it...
				foreach
				(
					Fusion targetFusion in
					_generalFusions
						.Where
						(
							fusion =>
								fusion.ResultantCard.Equals(targetCard)
						)
				)
				{
					//Get similar fusions to that of the one being checked
					//and determine the next strongest card that can be made
					Card nextStrongestResultantCard =
						_generalFusions
							.Where
							(
								similarFusion =>
									similarFusion.ResultantCard.AttackPoints <
										targetFusion.ResultantCard.AttackPoints &&
									(
										(
											targetFusion.TargetCard != null &&
											targetFusion.TargetCard.Equals(similarFusion.TargetCard)
										) ||
										targetFusion.TargetMonsterType == similarFusion.TargetMonsterType
									) &&
									(
										(
											targetFusion.FusionMaterialCard != null &&
											targetFusion.FusionMaterialCard
												.Equals(similarFusion.FusionMaterialCard)
										) ||
										targetFusion.FusionMaterialMonsterType == 
											similarFusion.FusionMaterialMonsterType
									)
							)
							.OrderByDescending
							(
								similarFusion =>
									similarFusion.ResultantCard.AttackPoints ?? 0
							)
							.FirstOrDefault()?
							.ResultantCard;

					//Determine which available cards can be used as the target and fusion materials.
					IEnumerable<KeyValuePair<Card, int>> viableTargetCards =
						availableCardCounts
							.Where
							(
								cardCount =>
									cardCount.Value > 0 &&
									(
										(
											targetFusion.TargetCard != null &&
											cardCount.Key.Equals(targetFusion.TargetCard)
										) ||
										(
											targetFusion.TargetCard == null &&
											CheckFusionTypeCompatibility
											(
												targetFusion.TargetMonsterType,
												cardCount.Key
											)
										)
									)
							)
							.OrderByDescending
							(
								cardCount =>
									cardCount.Key.AttackPoints
							);

					IEnumerable<KeyValuePair<Card, int>> viableFusionMaterialCards =
						availableCardCounts
							.Where
							(
								cardCount =>
									cardCount.Value > 0 &&
									(
										(
											targetFusion.FusionMaterialCard != null &&
											cardCount.Key.Equals(targetFusion.FusionMaterialCard)
										) ||
										(
											targetFusion.FusionMaterialCard == null &&
											CheckFusionTypeCompatibility
											(
												targetFusion.FusionMaterialMonsterType,
												cardCount.Key
											)
										)
									)
							)
							.OrderByDescending
							(
								cardCount =>
									cardCount.Key.AttackPoints
							);

					if
					(
						!viableTargetCards.Any() ||
						!viableFusionMaterialCards.Any() ||
						(
							viableTargetCards.None
							(
								targetCard => 
									targetCard.Key.AttackPoints > 
										(nextStrongestResultantCard?.AttackPoints ?? 0) 
							) &&
							viableFusionMaterialCards.None
							(
								fusionMaterialCard =>
									fusionMaterialCard.Key.AttackPoints > 
										(nextStrongestResultantCard?.AttackPoints ?? 0)
							)
						)
					)
					{
						//If the player is lacking either a target or fusion material 
						//card strong enough for the fusion, skip over the fusion.
						continue;
					}
					else
					{
						//If the player has cards strong enough to create the
						//target fusion, select the strongest ones available.
						Card selectedTargetCard = viableTargetCards.First().Key;
						Card selectedFusionMaterialCard = viableFusionMaterialCards.First().Key;

						if
						(
							selectedTargetCard.Equals(selectedFusionMaterialCard) &&
							availableCardCounts[selectedTargetCard] < 2
						)
						{
							//If both fusion material cards are the same card, but the player
							//only has access to one copy of it, take the next strongest card
							//as either the target or fusion material (if one is available)
							Card nextStrongestTarget = 
								viableTargetCards.Skip(1).FirstOrDefault().Key;

							Card nextStrongestFusionMaterial =
								viableFusionMaterialCards.Skip(1).First().Key;

							if(nextStrongestTarget == null && nextStrongestFusionMaterial == null)
							{
								//If only one viable target and fusion material card was available
								//(and they are both the same single card) skip the current fusion.
								continue;
							}
							else if
							(
								nextStrongestTarget.AttackPoints >= 
									nextStrongestFusionMaterial.AttackPoints
							)
							{
								selectedTargetCard = nextStrongestTarget;
							}
							else
							{
								selectedFusionMaterialCard = nextStrongestFusionMaterial;
							}
						}

						//Add the target and fusion material cards to the output list
						//and deduct them from the player's available card count.
						generalFusionCards.Add(selectedTargetCard);
						generalFusionCards.Add(selectedFusionMaterialCard);

						availableCardCounts[selectedTargetCard]--;
						availableCardCounts[selectedFusionMaterialCard]--;
					}
				}
			}

			return generalFusionCards;
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
