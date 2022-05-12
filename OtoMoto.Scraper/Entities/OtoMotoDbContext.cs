using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OtoMoto.Scraper.Entities
{
    public class OtoMotoDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SearchLink> SearchLinks { get; set; }
        public DbSet<AdLink> AdLinks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-NH3H4LM\SQLEXPRESS;Database=OtoMotoDb;Trusted_Connection=True;");
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}