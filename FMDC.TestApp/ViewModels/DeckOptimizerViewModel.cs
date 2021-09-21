using FMDC.BusinessLogic;
using FMDC.Model.Base;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMDC.TestApp.ViewModels
{
	public class DeckOptimizerViewModel : ObservableModel
	{
		#region Non-Public Member(s)
		private IFusionService _fusionService;

		private List<CardCount> _availableCardCounts;
		#endregion



		#region Constructor(s)
		public DeckOptimizerViewModel
		(
			App currentAppInstance,
			IFusionService fusionService
		)
		{
			_fusionService = fusionService;
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
					.ToList();
		}


		public void OptimizeDeck()
		{
			
		}
		#endregion
	}
}
