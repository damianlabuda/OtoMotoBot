using Microsoft.EntityFrameworkCore;

namespace Shared.Entities
{
    public class OtoMotoContext : DbContext
    {
        public OtoMotoContext(DbContextOptions<OtoMotoContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SearchLink> SearchLinks { get; set; }
        public DbSet<AdLink> AdLinks { get; set; }
        public DbSet<AdPrice> AdPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
