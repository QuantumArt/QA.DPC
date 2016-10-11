using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public class NotificationChannelDescriptor
    {
        public string Name { get; set; }
        public string LastStatus { get; set; }

        public DateTime? LastQueued { get; set; }
        public DateTime? LastPublished { get; set; }
        public int? LastId { get; set; }
    }
}
