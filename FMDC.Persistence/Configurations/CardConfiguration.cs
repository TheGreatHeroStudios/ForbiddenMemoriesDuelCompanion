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

			//Key included in parsed data (non-identity key)
			builder.Property(card => card.CardId).ValueGeneratedNever();

			//Configure Navigation Propert(ies)
			builder
				.HasOne(card => card.CardImage)
				.WithOne()
				.HasForeignKey<Card>(card => card.CardImageId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasOne(card => card.CardDescriptionImage)
				.WithOne()
				.HasForeignKey<Card>(card => card.CardDescriptionImageId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(card => card.EquippableCards)
				.WithOne(equippable => equippable.TargetCard)
				.HasForeignKey(equippable => equippable.TargetCardId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.HasMany(card => card.SecondaryTypes)
				.WithOne()
				.HasForeignKey(secondaryType => secondaryType.CardId)
				.OnDelete(DeleteBehavior.NoAction);
		}
		#endregion
	}
}
