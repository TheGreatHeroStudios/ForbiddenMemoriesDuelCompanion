using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
				.HasForeignKey(fusion => fusion.TargetCardId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasOne(fusion => fusion.FusionMaterialCard)
				.WithMany()
				.HasForeignKey(fusion => fusion.FusionMaterialCardId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasOne(fusion => fusion.ResultantCard)
				.WithMany()
				.HasForeignKey(fusion => fusion.ResultantCardId)
				.OnDelete(DeleteBehavior.NoAction);
		}
		#endregion
	}
}
