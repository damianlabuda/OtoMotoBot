namespace Shared.Entities
{
    public class AdLink
    {
        public long Id { get; set; }
        public int CategoryId { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Thumbnail { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Gearbox { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public int EngineCapacity { get; set; }
        public string FuelType { get; set; }
        public virtual List<AdPrice> Prices { get; set; }
        public virtual List<SearchLink> SearchLinks { get; set; }
        public int HowManyTimesHasNotInSearch { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
}