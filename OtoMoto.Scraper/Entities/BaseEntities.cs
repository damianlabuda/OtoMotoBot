using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtoMoto.Scraper.Entities
{ 
    public class BaseEntities
    {
        public int Id { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public DateTime LastUpdateDateTime { get; set; } = DateTime.Now;
    }
}