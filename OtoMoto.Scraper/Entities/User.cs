using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoMoto.Scraper.Entities
{
    public class User : BaseEntities
    {
        public virtual List<SearchLink> SearchLinks { get; set; }
    }
}
