using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Entities.Configurations
{
    public class SearchLinkConfiguration : IEntityTypeConfiguration<SearchLink>
    {
        public void Configure(EntityTypeBuilder<SearchLink> builder)
        {
            builder.HasMany(x => x.AdLinks)
                .WithOne(x => x.SearchLink)
                .HasForeignKey(x => x.SearchLinkId);

            builder.Property(x => x.Link)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.SearchCount)
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedDateTime)
                .HasDefaultValueSql("getutcdate()");

            builder.Property(x => x.LastUpdateDateTime)
                .ValueGeneratedOnUpdate();
        }
    }
}
