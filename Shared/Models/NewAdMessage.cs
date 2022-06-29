using Shared.Entities;

namespace Shared.Models
{
    public class NewAdMessage
    {
        public long Id { get; set; }
        public double Price { get; set; }
        public double PriceBefore { get; set; }
        public List<User> Users { get; set; }
    }
}
