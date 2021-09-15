using FMDC.Model.Enums;
using FMDC.Model.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FMDC.TestApp.ViewModels
{
	public class TrunkViewModel : INotifyPropertyChanged
	{
		#region Non-Public Member(s)
		private List<Card> _cardList;
		private List<GameImage> _monsterTypeImages;
		#endregion



		#region Constructor(s)
		public TrunkViewModel(App currentAppInstance)
		{
			_cardList = currentAppInstance.CardList;

			_monsterTypeImages =
				currentAppInstance.GameImages
					.Where
					(
						gameImage =>
							gameImage.EntityType == ImageEntityType.MonsterType
					)
					.ToList();
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}
}
