using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticRequestParams
    {
        private string _token;
        public ElasticRequestParams(HttpMethod verb, string indexName, string operation, string token, string type, bool isRequestSystemMethod)
        {
            ThrowNotFound = true;
            Verb = verb;
            Operation = operation;
            Type = type;
            _token = token;
            IndexName = indexName;
            UrlParams = new Dictionary<string, string>();
            IsRequestSystemMethod = isRequestSystemMethod;
        }

        public string Type { get; set; }
        
        public string Operation { get; set; }
        
        public HttpMethod Verb { get; set; }
        
        public string IndexName { get; set; }
        
        public bool ThrowNotFound { get; set; }

        public bool IsRequestSystemMethod { get; set; }


        public Dictionary<string, string> UrlParams;

        public string GetToken()
        {
            return _token;
        }

        public string GetUri()
        {
            var sb = new StringBuilder();
            var isGlobalOperation = Operation == "_bulk";
            if (!string.IsNullOrEmpty(IndexName) && !IsRequestSystemMethod)
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

            if (!string.IsNullOrEmpty(IndexName) && IsRequestSystemMethod)
            {
                if (!isGlobalOperation)
                {
                    sb.Append("/");
                }
                sb.Append(IndexName);
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