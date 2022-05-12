using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtoMoto.Scraper.Entities;

namespace OtoMoto.Scraper
{
    public class UserSeeder
    {
        public void Seed()
        {
            using (var context = new OtoMotoDbContext())
            {
                if (context.Database.CanConnect())
                {
                    if (!context.Users.Any())
                    {
                        var users = GetUsers();
                        context.Users.AddRange(users);
                        context.SaveChanges();
                    }
                }
            }
        }

        private IEnumerable<User> GetUsers()
        {
            var users = new List<User>()
            {
                new User()
                {
                    SearchLinks = new List<SearchLink>()
                    {
                        new SearchLink()
                        {
                            Link = "https://www.otomoto.pl/osobowe/toyota/corolla?search%5Bfilter_enum_generation%5D=gen-seria-e16-2012&search%5Bfilter_float_engine_power%3Afrom%5D=120&search%5Bfilter_enum_gearbox%5D=automatic&search%5Badvanced_search_expanded%5D=true"
                        }
                    }
                }
            };

            return users;
        }
    }
}