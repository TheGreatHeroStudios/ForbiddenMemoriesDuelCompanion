using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class CardPercentage
	{
		public int CardId { get; set; }
		public PercentageType PercentageDiscriminator { get; set; }
		public double GenerationPercentage { get; set; }
		public int GenerationRatePer2048 { get; set; }
	}
}
