using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Entities.Configurations
{
    public class AdLinkConfiguration : IEntityTypeConfiguration<AdLink>
    {
        public void Configure(EntityTypeBuilder<AdLink> builder)
        {
            // builder.Property(x => x.Prices)
            //     .IsRequired();

            builder.HasMany(x => x.Prices)
                .WithOne(x => x.AdLink)
                .HasForeignKey(x => x.AdLinkId);
            
            builder.Property(x => x.HowManyTimesHasNotInSearch)
                .HasDefaultValue(0);
            
            builder.Property(x => x.CreatedDateTime)
                // .HasDefaultValueSql("now() at time zone 'utc'");
                .HasDefaultValueSql("now()");
        }
    }
}
