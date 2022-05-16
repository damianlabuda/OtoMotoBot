using Microsoft.EntityFrameworkCore;

namespace Shared.Entities
{
    public class OtomotoSearchAuctions : DbContext
    {
        public OtomotoSearchAuctions(DbContextOptions<OtomotoSearchAuctions> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SearchLink> SearchLinks { get; set; }
        public DbSet<AdLink> AdLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
