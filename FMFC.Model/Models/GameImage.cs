using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class GameImage
	{
		public int EntityId { get; set; }
		public ImageEntityType EntityType { get; set; }
		public string ImageRelativePath { get; set; }
	}
}
