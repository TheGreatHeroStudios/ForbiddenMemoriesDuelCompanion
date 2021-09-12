using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMDC.Persistence.Configurations
{
	public class CardPercentageConfiguration : IEntityTypeConfiguration<CardPercentage>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<CardPercentage> builder)
		{
			//Configure Table Name
			builder.ToTable("CardPercentage");

			//Configure Primary Key
			builder.HasKey(cardPercentage => cardPercentage.CardPercentageId);

			//Configure Navigation Propert(ies)
			builder
				.HasOne(cardPercentage => cardPercentage.Character)
				.WithMany(character => character.CardPercentages)
				.HasForeignKey(cardPercentage => cardPercentage.CharacterId);

			builder
				.HasOne(cardPercentage => cardPercentage.Card)
				.WithMany()
				.HasForeignKey(cardPercentage => cardPercentage.CardId);
		}
		#endregion
	}
}
