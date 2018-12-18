using System;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public class ParseJsonException: Exception
    {
        public ParseJsonException() : base() {}

        public ParseJsonException(string message) : base(message) {}

        public ParseJsonException(string message, Exception inner) : base(message, inner) {}
        
        public string Json { get; set; }
        

    }
}