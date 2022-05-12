using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoMoto.Scraper.Entities.LinkStructure
{
    public class LinkExtensions
    {
        public Persistedquery persistedQuery { get; set; } = new Persistedquery();
    }

    public class Persistedquery
    {
        public string sha256Hash { get; set; } = "68185c38c934469b1c93bedd9aec361086b91b10a4efbbae682ee05229899e13";
        public int version { get; set; } = 1;
    }
}
