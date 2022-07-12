namespace Shared.Entities;

public class AdPrice
{
    public Guid Id { get; set; }
    public double Price { get; set; }
    public string Currency { get; set; }
    public DateTime CreatedDateTime { get; set; }
    
    public virtual AdLink AdLink { get; set; }
    public long AdLinkId { get; set; }
}