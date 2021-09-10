using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Persistence.Configurations
{
	public class FusionConfiguration : IEntityTypeConfiguration<Fusion>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<Fusion> builder)
		{
			//Configure Table Name
			builder.ToTable("Fusion");

			//Configure Primary Key
			builder.HasKey(fusion => fusion.FusionId);

			//Configure Navigation Propert(ies)
			builder
				.HasOne(fusion => fusion.TargetCard)
				.WithMany()
				.HasForeignKey(fusion => fusion.TargetCardId);

			builder
				.HasOne(fusion => fusion.FusionMaterialCard)
				.WithMany()
				.HasForeignKey(fusion => fusion.FusionMaterialCardId);

			builder
				.HasOne(fusion => fusion.ResultantCard)
				.WithMany()
				.HasForeignKey(fusion => fusion.ResultantCardId);
		}
		#endregion
	}
}
