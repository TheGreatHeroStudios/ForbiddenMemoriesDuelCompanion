using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FMDC.Model
{
	public static class ModelExtensions
	{
		//Guardian Star Extensions
		public static GuardianStar StrongAgainst(this GuardianStar source)
		{
			switch (source)
			{
				case Enums.GuardianStar.Sun: return Enums.GuardianStar.Moon;
				case Enums.GuardianStar.Moon: return Enums.GuardianStar.Venus;
				case Enums.GuardianStar.Mercury: return Enums.GuardianStar.Sun;
				case Enums.GuardianStar.Venus: return Enums.GuardianStar.Mercury;
				case Enums.GuardianStar.Mars: return Enums.GuardianStar.Jupiter;
				case Enums.GuardianStar.Jupiter: return Enums.GuardianStar.Saturn;
				case Enums.GuardianStar.Saturn: return Enums.GuardianStar.Uranus;
				case Enums.GuardianStar.Uranus: return Enums.GuardianStar.Pluto;
				case Enums.GuardianStar.Neptune: return Enums.GuardianStar.Mars;
				case Enums.GuardianStar.Pluto: return Enums.GuardianStar.Neptune;
				default: return Enums.GuardianStar.Unknown;
			}
		}

		public static GuardianStar WeakAgainst(this GuardianStar source)
		{
			switch (source)
			{
				case Enums.GuardianStar.Sun: return Enums.GuardianStar.Mercury;
				case Enums.GuardianStar.Moon: return Enums.GuardianStar.Sun;
				case Enums.GuardianStar.Mercury: return Enums.GuardianStar.Venus;
				case Enums.GuardianStar.Venus: return Enums.GuardianStar.Moon;
				case Enums.GuardianStar.Mars: return Enums.GuardianStar.Neptune;
				case Enums.GuardianStar.Jupiter: return Enums.GuardianStar.Mars;
				case Enums.GuardianStar.Saturn: return Enums.GuardianStar.Jupiter;
				case Enums.GuardianStar.Uranus: return Enums.GuardianStar.Saturn;
				case Enums.GuardianStar.Neptune: return Enums.GuardianStar.Pluto;
				case Enums.GuardianStar.Pluto: return Enums.GuardianStar.Uranus;
				default: return Enums.GuardianStar.Unknown;
			}
		}

		public static Alignment Alignment(this GuardianStar source)
		{
			switch (source)
			{
				case Enums.GuardianStar.Sun: return Enums.Alignment.White;
				case Enums.GuardianStar.Moon: return Enums.Alignment.Demon;
				case Enums.GuardianStar.Mercury: return Enums.Alignment.Black;
				case Enums.GuardianStar.Venus: return Enums.Alignment.Illusion;
				case Enums.GuardianStar.Mars: return Enums.Alignment.Fire;
				case Enums.GuardianStar.Jupiter: return Enums.Alignment.Forest;
				case Enums.GuardianStar.Saturn: return Enums.Alignment.Wind;
				case Enums.GuardianStar.Uranus: return Enums.Alignment.Earth;
				case Enums.GuardianStar.Neptune: return Enums.Alignment.Water;
				case Enums.GuardianStar.Pluto: return Enums.Alignment.Thunder;
				default: return Enums.Alignment.Unknown;
			}
		}


		//Alignment Extensions
		public static Alignment StrongAgainst(this Alignment source)
		{
			return source.GuardianStar().StrongAgainst().Alignment();
		}

		public static Alignment WeakAgainst(this Alignment source)
		{
			return source.GuardianStar().WeakAgainst().Alignment();
		}

		public static GuardianStar GuardianStar(this Alignment source)
		{
			switch (source)
			{
				case Enums.Alignment.White: return Enums.GuardianStar.Sun;
				case Enums.Alignment.Demon: return Enums.GuardianStar.Moon;
				case Enums.Alignment.Black: return Enums.GuardianStar.Mercury;
				case Enums.Alignment.Illusion: return Enums.GuardianStar.Venus;
				case Enums.Alignment.Fire: return Enums.GuardianStar.Mars;
				case Enums.Alignment.Forest: return Enums.GuardianStar.Jupiter;
				case Enums.Alignment.Wind: return Enums.GuardianStar.Saturn;
				case Enums.Alignment.Earth: return Enums.GuardianStar.Uranus;
				case Enums.Alignment.Water: return Enums.GuardianStar.Neptune;
				case Enums.Alignment.Thunder: return Enums.GuardianStar.Pluto;
				default: return Enums.GuardianStar.Unknown;
			}
		}


		//Terrain Extensions
		public static IEnumerable<MonsterType> BenefitsTypes(this Terrain terrain)
		{
			List<MonsterType> monsterTypes = new List<MonsterType>();

			switch (terrain)
			{
				case Terrain.Forest:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Beast,
							MonsterType.BeastWarrior,
							MonsterType.Insect,
							MonsterType.Plant
						}
					);
					break;
				}
				case Terrain.Wasteland:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Zombie,
							MonsterType.Dinosaur,
							MonsterType.Rock
						}
					);
					break;
				}
				case Terrain.Mountain:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Dragon,
							MonsterType.WingedBeast,
							MonsterType.Thunder
						}
					);
					break;
				}
				case Terrain.Grassland:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Beast,
							MonsterType.BeastWarrior
						}
					);
					break;
				}
				case Terrain.Sea:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Aqua,
							MonsterType.Thunder
						}
					);
					break;
				}
				case Terrain.Darkness:
				{
					monsterTypes.AddRange
					(
						new List<MonsterType>
						{
							MonsterType.Spellcaster,
							MonsterType.Fiend
						}
					);
					break;
				}
			}

			return monsterTypes;
		}


		//Card Extensions
		public static IEnumerable<Terrain> FavorableTerrains(this Card card)
		{
			List<Terrain> favorableTerrains = new List<Terrain>();

			//For each of the terrains, check to see if it benefits the monster type of
			//the provided card. If so, add it to the list of favorable terrains for the card.
			((IEnumerable<Terrain>)Enum.GetValues(typeof(Terrain)))
				.ToList()
				.ForEach
				(
					terrain =>
					{
						if (terrain.BenefitsTypes().Contains(card.MonsterType ?? MonsterType.Unknown))
						{
							favorableTerrains.Add(terrain);
						}
					}
				);

			return favorableTerrains;
		}

		public static IEnumerable<Terrain> UnfavorableTerrains(this Card card)
		{
			List<Terrain> unfavorableTerrains = new List<Terrain>();

			if (new List<MonsterType> { MonsterType.Machine, MonsterType.Pyro }.Contains(card.MonsterType ?? MonsterType.Unknown))
			{
				unfavorableTerrains.Add(Terrain.Sea);
			}
			else if (card.MonsterType == MonsterType.Fairy)
			{
				unfavorableTerrains.Add(Terrain.Darkness);
			}

			return unfavorableTerrains;
		}
	}
}
