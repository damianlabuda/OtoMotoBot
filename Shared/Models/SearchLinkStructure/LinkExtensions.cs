namespace Shared.Models.SearchLinkStructure
{
    public class LinkExtensions
    {
        public Persistedquery persistedQuery { get; set; } = new Persistedquery();
    }

    public class Persistedquery
    {
        public string sha256Hash { get; set; }
        public int version { get; set; } = 1;
    }
}
