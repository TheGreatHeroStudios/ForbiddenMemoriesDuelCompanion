using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.BusinessLogic
{
	public interface IFusionService
	{
		List<List<Card>> GenerateFusionPermutations
		(
			List<Card> currentPermutation,
			IEnumerable<Card> potentialFusionMaterialCards
		);
	}
}
