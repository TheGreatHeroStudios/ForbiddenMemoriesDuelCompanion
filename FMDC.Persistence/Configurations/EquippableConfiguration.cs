using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Persistence.Configurations
{
	public class EquippableConfiguration : IEntityTypeConfiguration<Equippable>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<Equippable> builder)
		{
			//Configure Table Name
			builder.ToTable("Equippable");

			//Configure Primary Key
			builder.HasKey(equippable => equippable.EquippableId);

			//Configure Navigation Propert(ies)
			builder
				.HasOne(equippable => equippable.TargetCard)
				.WithMany(card => card.EquippableCards)
				.HasForeignKey(equippable => equippable.TargetCardId);

			builder
				.HasOne(equippable => equippable.EquipCard)
				.WithMany()
				.HasForeignKey(equippable => equippable.EquipCardId);
		}
		#endregion
	}
}
