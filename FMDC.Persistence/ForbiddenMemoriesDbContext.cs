using FMDC.Model;
using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TGH.Common.Persistence.Implementations;

namespace FMDC.Persistence
{
	public class ForbiddenMemoriesDbContext : SqliteDbContext
	{
		#region Constructor(s)
		public ForbiddenMemoriesDbContext(string targetDatabaseFilePath) :
			base
			(
				targetDatabaseFilePath,
				PersistenceConstants.SQLITE_TEMPLATE_FILEPATH,
				Assembly.GetExecutingAssembly()
			)
		{

		}
		#endregion



		#region DbSet Propert(ies)
		public DbSet<GameImage> GameImages { get; set; }
		public DbSet<Card> Cards { get; set; }
		public DbSet<SecondaryType> SecondaryTypes { get; set; }
		public DbSet<Equippable> Equippables { get; set; }
		public DbSet<Fusion> Fusions { get; set; }
		public DbSet<Character> Characters { get; set; }
		public DbSet<CardPercentage> CardPercentages { get; set; }
		#endregion
	}
}
