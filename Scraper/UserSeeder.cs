using Shared.Entities;

namespace Scraper
{
    public class UserSeeder
    {
        private readonly IServiceScopeFactory _iServiceScopeFactory;

        public UserSeeder(IServiceScopeFactory iServiceScopeFactory)
        {
            _iServiceScopeFactory = iServiceScopeFactory;
        }

        public void Seed()
        {
            using (var scope = _iServiceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OtoMotoContext>();

                if (context.Database.CanConnect())
                {
                    if (!context.Users.Any())
                    {
                        var user = GetUser();
                        context.Users.Add(user);
                        context.SaveChanges();
                    }
                }
            }
        }

        private User GetUser()
        {
            var searchLink = new SearchLink()
            {
                Link = "https://www.otomoto.pl/osobowe/audi/a3?search%5Badvanced_search_expanded%5D=true"
            };

            var user = new User();

            user.SearchLinks.Add(searchLink);

            return user;
        }
    }
}
