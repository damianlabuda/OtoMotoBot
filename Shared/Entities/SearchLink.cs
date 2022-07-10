using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities
{
    public class SearchLink
    { 
        public Guid Id { get; set; }
        public string Link { get; set; }
        public int SearchCount { get; set; }
        public int AdLinksCount { get; set; }
        public virtual List<User> Users { get; set; }
        public virtual List<AdLink> AdLinks { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
    }
}
