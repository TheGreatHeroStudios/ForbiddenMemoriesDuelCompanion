using FMDC.Model.Enums;

namespace FMDC.Model.Models
{
	public class GameImage
	{
		public ImageEntityType EntityType { get; set; }
		public int EntityId { get; set; }
		public string ImageBase64 { get; set; }
	}
}
