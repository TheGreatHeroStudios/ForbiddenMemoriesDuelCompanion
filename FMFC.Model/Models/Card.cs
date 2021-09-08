using FMDC.Model.Enums;
using System.Collections.Generic;

namespace FMDC.Model.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CardType CardType { get; set; }
        public MonsterType? MonsterType { get; set; }
		public string Description { get; set; }
		public GuardianStar? FirstGuardianStar { get; set; }
		public GuardianStar? SecondGuardianStar { get; set; }
		public int? Level { get; set; }
        public int? Attack { get; set; }
        public int? Defense { get; set; }
		public string Password { get; set; }
		public int? StarchipCost { get; set; }
		public GameImage CardImage { get; set; }
		public GameImage CardDescriptionImage { get; set; }
		public IEnumerable<SecondaryType> SecondaryTypes { get; set; }
		public IEnumerable<Equippable> EquippableCards { get; set; }
	}
}
