using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FMDC.Persistence.Configurations
{
	public class CharacterConfiguration : IEntityTypeConfiguration<Character>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<Character> builder)
		{
			//Configure Table Name
			builder.ToTable("Character");

			//Configure Primary Key
			builder.HasKey(character => character.CharacterId);

			//Configure Navigation Propert(ies)
			builder
				.HasOne(character => character.CharacterImage)
				.WithOne()
				.HasForeignKey<Character>(character => character.CharacterImageId);

			builder
				.HasMany(character => character.CardPercentages)
				.WithOne(cardPercentage => cardPercentage.Character)
				.HasForeignKey(cardPercentage => cardPercentage.CharacterId);
		}
		#endregion
	}
}
