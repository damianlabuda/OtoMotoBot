using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Entities.Configurations;

public class AdPriceConfiguration : IEntityTypeConfiguration<AdPrice>
{
    public void Configure(EntityTypeBuilder<AdPrice> builder)
    {
        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.Currency)
            .IsRequired();

        builder.Property(x => x.CreatedDateTime)
            .HasDefaultValueSql("now()");
    }
}