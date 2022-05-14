namespace Shared.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public virtual List<SearchLink> SearchLinks { get; set; }
        public long? TelegramChatId { get; set; }
        public string? TelegramName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
}
