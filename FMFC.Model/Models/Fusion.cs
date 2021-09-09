using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class Fusion
	{
		public int FusionId { get; set; }
		public FusionType FusionType { get; set; }
		public int? TargetCardId { get; set; }
		public MonsterType? TargetCardType { get; set; }
		public int? FusionMaterialCardId { get; set; }
		public MonsterType? FusionMaterialCardType { get; set; }
		public int ResultantCardId { get; set; }
	}
}
