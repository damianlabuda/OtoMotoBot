using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace OtoMoto.Scraper.Entities
{
    public class SearchLink : BaseEntities
    {
        public string Link { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<AdLink> AdLinks { get; set; }
    }
}
