﻿using FMDC.Model.Enums;
using System.Collections.Generic;

namespace FMDC.Model.Models
{
	public class Card
	{
		#region Propert(ies)
		public int CardId { get; set; }
		public string Name { get; set; }
		public CardType CardType { get; set; }
		public MonsterType? MonsterType { get; set; }
		public string Description { get; set; }
		public GuardianStar? FirstGuardianStar { get; set; }
		public GuardianStar? SecondGuardianStar { get; set; }
		public int? Level { get; set; }
		public int? AttackPoints { get; set; }
		public int? DefensePoints { get; set; }
		public string Password { get; set; }
		public int? StarchipCost { get; set; }
		#endregion



		#region Navigation Propert(ies)
		public List<CardPercentage> CardPercentages { get; set; }
		public GameImage CardImage { get; set; }
		public GameImage CardDescriptionImage { get; set; }
		public List<SecondaryType> SecondaryTypes { get; set; }
		public List<Equippable> EquippableCards { get; set; }
		#endregion
	}
}
