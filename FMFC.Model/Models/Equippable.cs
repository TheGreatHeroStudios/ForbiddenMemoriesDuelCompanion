namespace FMDC.Model.Models
{
	public class Equippable
	{
		#region Propert(ies)
		public int EquippableId { get; set; }
		public int TargetCardId { get; set; }
		public int EquipCardId { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public Card TargetCard { get; set; }
		public Card EquipCard { get; set; }
		#endregion
	}
}
