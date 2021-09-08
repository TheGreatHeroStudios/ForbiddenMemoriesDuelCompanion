using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class Fusion
	{
		public FusionType FusionType { get; set; }
		public int? TargetCardId { get; set; }
		public MonsterType? TargetCardType { get; set; }
		public int? FusionMaterialCardId { get; set; }
		public MonsterType? FusionMaterialCardType { get; set; }
		public int ResultantCardId { get; set; }
	}
}
