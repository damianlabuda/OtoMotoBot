using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoMoto.Scraper.Entities
{
    public class AdLink : BaseEntities
    {
        public string Link { get; set; }
        public double Price { get; set; }

        public int SearchLinkId { get; set; }
        public virtual SearchLink SearchLink { get; set; }
    }
}
