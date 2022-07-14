using Shared.Entities;

namespace Shared.Models
{
    public class NewAdMessage
    {
        public long Id { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public double PriceBefore { get; set; }
        public string CurrencyBefore { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Gearbox { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public int EngineCapacity { get; set; }
        public string FuelType { get; set; }
        public List<User> Users { get; set; }
    }
}
