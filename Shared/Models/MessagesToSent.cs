using Shared.Entities;

namespace Shared.Models
{
    public class MessagesToSent
    {
        public List<NewAdMessage> NewAdMessages { get; set; }
        public List<User> Users { get; set; }
    }
}
