﻿using FMDC.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class CharacterCardPercentage
	{
		#region Propert(ies)
		public int CharacterCardPercentageId { get; set; }
		public int CharacterId { get; set; }
		public int CardId { get; set; }
		public PercentageType PercentageType { get; set; }
		public double GenerationPercentage { get; set; }
		public int GenerationRatePer2048 { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public Character Character { get; set; }
		public Card Card { get; set; }
		#endregion
	}
}
