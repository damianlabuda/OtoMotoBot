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

//stare 68185c38c934469b1c93bedd9aec361086b91b10a4efbbae682ee05229899e13
//nowe 082392db93e737b5af878c4cdb40cdfeb76a9751bf2b9d8992c2b7c048dd3c61
