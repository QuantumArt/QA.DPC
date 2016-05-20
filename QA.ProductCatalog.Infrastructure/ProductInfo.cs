using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.ProductCatalog.Infrastructure
{
    public class ProductInfo
    {
        public int Id { get; set; }

        public DateTime Updated { get; set; }

        public string Alias { get; set; }

        public string Title { get; set; }

        public string Hash { get; set; }

		public string LastPublishedUserName { get; set; }

		public int? LastPublishedUserId { get; set; }
    }
}
