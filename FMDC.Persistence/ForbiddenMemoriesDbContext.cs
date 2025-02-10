using FMDC.Model;
using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TGH.Common.Persistence.Implementations;

namespace FMDC.Persistence
{
	public class ForbiddenMemoriesDbContext : EFCoreDatabaseContextBase
	{
		#region DbSet Propert(ies)
		public DbSet<GameImage> GameImages { get; set; }
		public DbSet<Card> Cards { get; set; }
		public DbSet<SecondaryType> SecondaryTypes { get; set; }
		public DbSet<Equippable> Equippables { get; set; }
		public DbSet<Fusion> Fusions { get; set; }
		public DbSet<Character> Characters { get; set; }
		public DbSet<CardPercentage> CardPercentages { get; set; }
		#endregion



		#region Override(s)
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSqlLocalDb;Database=FMDC;Trusted_Connection=True;");

			base.OnConfiguring(optionsBuilder);
		}


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

			base.OnModelCreating(modelBuilder);
		}
		#endregion
	}
}
