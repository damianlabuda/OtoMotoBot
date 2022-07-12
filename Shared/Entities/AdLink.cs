namespace Shared.Entities
{
    public class AdLink
    {
        public long Id { get; set; }
        public virtual List<AdPrice> Prices { get; set; }
        public virtual List<SearchLink> SearchLinks { get; set; }
        public int HowManyTimesHasNotInSearch { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
}