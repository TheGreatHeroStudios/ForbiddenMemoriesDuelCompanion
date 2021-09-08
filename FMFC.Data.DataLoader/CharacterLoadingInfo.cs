using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Data.DataLoader
{
	public struct CharacterLoadingInfo
	{
		public int CharacterId { get; set; }
		public string CharacterName { get; set; }
		public string InfoRelativePath { get; set; }
		public int DeckListOrdinal { get; set; }
		public string CharacterImagePath { get; set; }
	}
}
