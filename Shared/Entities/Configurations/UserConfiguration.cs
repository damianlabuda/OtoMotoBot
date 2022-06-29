using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shared.Entities.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasMany(x => x.SearchLinks)
                .WithMany(x => x.Users);

            builder.Property(x => x.TelegramChatNotFound)
                .HasDefaultValue(false);
            
            builder.Property(x => x.CreatedDateTime)
                // .HasDefaultValueSql("now() at time zone 'utc'");
                .HasDefaultValueSql("now()");
        }
    }
}
