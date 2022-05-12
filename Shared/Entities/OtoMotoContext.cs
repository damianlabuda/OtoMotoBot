using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
