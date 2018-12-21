using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticRequestParams
    {
        public ElasticRequestParams(HttpMethod verb, string indexName, string operation, string type)
        {
            
            Verb = verb;
            Operation = operation;
            Type = type;
            IndexName = indexName;
            UrlParams = new Dictionary<string, string>();            
        }
        
        
        public string Type { get; set; }
        
        public string Operation { get; set; }
        
        public HttpMethod Verb { get; set; }
        
        public string IndexName { get; set; }
        

        public Dictionary<string, string> UrlParams;
        
        
        public string GetUri()
        {
            var sb = new StringBuilder();
            var isGlobalOperation = Operation == "_bulk";
            if (!string.IsNullOrEmpty(IndexName))
            {
                if (!isGlobalOperation)
                {
                    sb.Append(IndexName);                   
                }
            }

            if (!string.IsNullOrEmpty(Type) && !isGlobalOperation)
            {
                sb.Append("/");
                sb.Append(Type);
            }
            
            if (!string.IsNullOrEmpty(Operation))
            {
                if (!isGlobalOperation)
                {
                    sb.Append("/");
                }
                sb.Append(Operation);
            }
            
            if (UrlParams.Any())
            {
                sb.Append("?");
                sb.Append(string.Join("&", UrlParams.Select(n => $"{n.Key}={n.Value}")));
            }
            
            return sb.ToString();
        }

    }
}