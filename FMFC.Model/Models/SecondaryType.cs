using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class SecondaryType
	{
		public int SecondaryTypeId { get; set; }
		public int CardId { get; set; }
		public MonsterType Type { get; set; }
	}
}
