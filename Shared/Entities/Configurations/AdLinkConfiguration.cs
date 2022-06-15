﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Entities.Configurations
{
    public class AdLinkConfiguration : IEntityTypeConfiguration<AdLink>
    {
        public void Configure(EntityTypeBuilder<AdLink> builder)
        {
            builder.HasKey(x => x.Link);

            builder.Property(x => x.Link)
                .IsRequired(true)
                .HasMaxLength(2048);

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property(x => x.CreatedDateTime)
                .HasDefaultValueSql("now() at time zone 'utc'");

            builder.Property(x => x.LastUpdateDateTime)
                .ValueGeneratedOnUpdate();
        }
    }
}
