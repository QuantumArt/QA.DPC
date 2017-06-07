using System;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class SonicError
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public Exception Exception { get; set; }
    }
}