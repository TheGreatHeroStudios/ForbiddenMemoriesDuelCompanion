using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Model.Models
{
	public class Character
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Biography { get; set; }
		public GameImage CharacterImage { get; set; }
		public IEnumerable<CardPercentage> DeckCards { get; set; }
		public IEnumerable<CardPercentage> DropRates { get; set; }
	}
}
