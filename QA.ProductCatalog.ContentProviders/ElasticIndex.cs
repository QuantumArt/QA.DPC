using System;

namespace QA.ProductCatalog.ContentProviders
{
	public class ElasticIndex
	{
		public string Name { get; set; }

        public string Url { get; set; }

		public string Language { get; set; }

	    public string State { get; set; }

        public bool IsDefault { get; set; }

        public bool DoTrace { get; set; }

        public string ReindexUrl { get; set; }

        public DateTime? Date { get; set; }
    }
}
