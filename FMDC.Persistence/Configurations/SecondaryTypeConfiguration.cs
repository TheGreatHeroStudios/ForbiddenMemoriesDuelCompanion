using FMDC.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Persistence.Configurations
{
	public class SecondaryTypeConfiguration : IEntityTypeConfiguration<SecondaryType>
	{
		#region 'IEntityTypeConfiguration' Implementation
		public void Configure(EntityTypeBuilder<SecondaryType> builder)
		{
			//Configure Table Name
			builder.ToTable("SecondaryType");

			//Configure Primary Key
			builder.HasKey(secondaryType => secondaryType.SecondaryTypeId);
		}
		#endregion
	}
}
