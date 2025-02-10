using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMDC.Persistence.Configurations
{
	public class GameImageConfiguration : IEntityTypeConfiguration<GameImage>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<GameImage> builder)
		{
			//Configure Table Name
			builder.ToTable("GameImage");

			//Configure Primary Key
			builder.HasKey(gameImage => gameImage.GameImageId);

			//Key included in parsed data (non-identity key)
			//builder.Property(gameImage => gameImage.GameImageId).ValueGeneratedNever();
		}
		#endregion
	}
}
