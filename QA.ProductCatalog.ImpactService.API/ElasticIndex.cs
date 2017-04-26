using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.ProductCatalog.ImpactService.API
{
    public class ElasticIndex
    {
        public const string DefaultState = "live";

        public const string DefaultLanguage = "invariant";

        public const string DefaultName = "products";


        public ElasticIndex()
        {
            State = DefaultState;
            Language = DefaultLanguage;
            Name = DefaultName;
        }

        public string State { get; set; }

        public string Language { get; set; }

        public string Name { get; set; }
    }
}
