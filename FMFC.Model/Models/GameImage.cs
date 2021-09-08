using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class GameImage
	{
		public ImageEntityType EntityType { get; set; }
		public int EntityId { get; set; }
		public string ImageBase64 { get; set; }
	}
}
