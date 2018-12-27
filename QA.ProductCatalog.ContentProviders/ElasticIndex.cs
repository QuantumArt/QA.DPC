using System;
using System.Linq;

namespace QA.ProductCatalog.ContentProviders
{
	public class ElasticIndex
	{
		public string Name { get; set; }

		public string Url
		{
			get => _url;
			set
			{
				_url = value;
				_urls = GetUrls();
			}
		}

		private string[] _urls;
        private string _url;

        private string[] GetUrls()
        {
	        return Url.Split(';').Select(n => n.Trim()).ToArray();
        }
        
		public string[] Urls => _urls ?? (_urls = GetUrls());

		public string Language { get; set; }

	    public string State { get; set; }

        public bool IsDefault { get; set; }

        public bool DoTrace { get; set; }

        public string ReindexUrl { get; set; }

        public DateTime? Date { get; set; }
    }
}
