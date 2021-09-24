using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Text;
using TGH.Common.DataStructures;

namespace FMDC.BusinessLogic
{
	public interface IFusionService
	{
		void BuildFusionTree
		(
			BalancedBinaryTree<Card> currentTree,
			List<Fusion> viableFusions,
			Dictionary<Card, int> availableCardCounts
		);


		/// <summary>
		///		Converts a set of cards representing intermediate steps
		///		involved in a fusion to a subset containing cards for display.
		/// </summary>
		/// <param name="fusionPermutation">
		///		The permutation of cards resulting in a fusion.
		/// </param>
		/// <param name="includeResultantCard">
		///		Should the card representing the 
		///		result of the fusion be displayed?
		/// </param>
		/// <param name="throwAwayFirstCardInSequence">
		///		Is the first card in the sequence meant to
		///		be sacrificed to begin a new fusion chain?
		/// </param>
		/// <returns>
		///		A 'scrubbed' list of cards showing just the material cards
		///		(and optionally the resultant card) involved in a fusion.
		/// </returns>
		List<Card> ConvertToPlaySequence
		(
			List<Card> fusionPermutation,
			bool includeResultantCard = true,
			bool throwAwayFirstCardInSequence = false
		);


		/// <summary>
		///		Generates a list of specific fusions that can possibly
		///		be made, based on a supplied set of available cards.
		/// </summary>
		/// <param name="availableCardCounts">
		///		A dictionary of available cards and the number of
		///		instances of that card which are available to the player.
		/// </param>
		/// <returns>
		///		A list of all fusions that can be made with the
		///		supplied set of <paramref name="availableCardCounts"/>
		/// </returns>
		List<Fusion> DetermineViableSpecificFusions
		(
			Dictionary<Card, int> availableCardCounts
		);


		/// <summary>
		///		Generates all possible fusion permutations which can
		///		be made, given a <paramref name="currentPermutation"/>
		///		and a set of <paramref name="potentialFusionMaterialCards"/>
		/// </summary>
		/// <remarks>
		///		This method is called recursively to generate permutations based
		///		on the supplied set of <paramref name="potentialFusionMaterialCards"/>.
		///		As such, supplying a large number of cards can result in poor performance.
		/// </remarks>
		/// <param name="currentPermutation">
		///		A chain of cards used as the start of a fusion chain.
		/// </param>
		/// <param name="potentialFusionMaterialCards">
		///		A list of cards available for adding mutations
		///		to the <paramref name="currentPermutation"/>.
		/// </param>
		/// <returns>
		///		All possible permutations which can be made from the
		///		<paramref name="currentPermutation"/> and the set of
		///		<paramref name="potentialFusionMaterialCards"/>.
		/// </returns>
		List<List<Card>> GenerateFusionPermutations
		(
			List<Card> currentPermutation,
			IEnumerable<Card> potentialFusionMaterialCards
		);
	}
}
