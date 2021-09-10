using System.Collections.Generic;

namespace FMDC.Model.Models
{
	public class Character
	{
		#region Propert(ies)
		public int CharacterId { get; set; }
		public int CharacterImageId { get; set; }
		public string Name { get; set; }
		public string Biography { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public GameImage CharacterImage { get; set; }
		public List<CharacterCardPercentage> CardPercentages { get; set; }
		#endregion
	}
}
