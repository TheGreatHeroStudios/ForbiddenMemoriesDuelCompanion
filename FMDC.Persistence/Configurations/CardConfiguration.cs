using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMDC.Persistence.Configurations
{
	public class CardConfiguration : IEntityTypeConfiguration<Card>
	{

		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<Card> builder)
		{
			//Configure Table Name
			builder.ToTable("Card");

			//Configure Primary Key
			builder.HasKey(card => card.CardId);

			//Configure Navigation Propert(ies)
			builder
				.HasOne(card => card.CardImage)
				.WithOne()
				.HasForeignKey<Card>(card => card.CardImageId);

			builder
				.HasOne(card => card.CardDescriptionImage)
				.WithOne()
				.HasForeignKey<Card>(card => card.CardDescriptionImageId);

			builder
				.HasMany(card => card.EquippableCards)
				.WithOne(equippable => equippable.TargetCard)
				.HasForeignKey(equippable => equippable.TargetCardId);

			builder
				.HasMany(card => card.SecondaryTypes)
				.WithOne()
				.HasForeignKey(secondaryType => secondaryType.CardId);
		}
		#endregion
	}
}
