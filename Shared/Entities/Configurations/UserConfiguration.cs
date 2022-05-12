using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            builder.Property(x => x.CreatedDateTime)
                .HasDefaultValueSql("getutcdate()");

            builder.Property(x => x.LastUpdateDateTime)
                .ValueGeneratedOnUpdate();
        }
    }
}
