namespace Shared.Entities
{
    public class AdLink
    {
        public Guid Id { get; set; }
        public string Link { get; set; }
        public double Price { get; set; }
        public virtual SearchLink SearchLink { get; set; }
        public Guid SearchLinkId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
}