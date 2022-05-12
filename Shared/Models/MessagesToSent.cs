using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Shared.Entities;

namespace Shared.Models
{
    public class MessagesToSent
    {
        public List<NewAdMessage> NewAdMessages { get; set; }
        public List<User> Users { get; set; }
    }
}
