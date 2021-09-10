using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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
		}
		#endregion
	}
}
