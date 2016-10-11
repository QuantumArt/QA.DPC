using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public class NotificationSenderChannel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string LastStatus { get; set; }
        public DateTime? LastPublication { get; set; }
        public int? LastId { get; set; }
    }
}
