using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class Fusion
	{
		#region Propert(ies)
		public int FusionId { get; set; }
		public FusionType FusionType { get; set; }
		public int? TargetCardId { get; set; }
		public MonsterType? TargetMonsterType { get; set; }
		public int? FusionMaterialCardId { get; set; }
		public MonsterType? FusionMaterialMonsterType { get; set; }
		public int ResultantCardId { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public Card TargetCard { get; set; }
		public Card FusionMaterialCard { get; set; }
		public Card ResultantCard { get; set; }
		#endregion



		#region Override(s)
		public override string ToString()
		{
			return
				FusionType == FusionType.Specific ?
					$"{TargetCard} + {FusionMaterialCard} = {ResultantCard}" :
					$"{TargetMonsterType?.ToString() ?? TargetCard?.ToString()} + " +
					$"{FusionMaterialMonsterType?.ToString() ?? FusionMaterialCard?.ToString()} = " +
					$"{ResultantCard}";
		}
		#endregion
	}
}
