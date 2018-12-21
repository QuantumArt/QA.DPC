using System;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticClientException : Exception
    {

        public string Request { get; set; }
        
        public string[] BaseUrls { get; set; }
        
        public ElasticRequestParams ElasticRequestParams { get; set; }
        
        public ElasticClientException()
        {
        }

        public ElasticClientException(string message) : base(message)
        {
        }

        public ElasticClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}