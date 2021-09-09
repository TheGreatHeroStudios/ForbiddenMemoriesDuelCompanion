using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class CardPercentage
	{
		public int CardPercentageId { get; set; }
		public int CardId { get; set; }
		public PercentageType PercentageType { get; set; }
		public double GenerationPercentage { get; set; }
		public int GenerationRatePer2048 { get; set; }
	}
}
