using QA.ProductCatalog.HighloadFront.Infrastructure;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class IndexerDecorator : IProductPostProcessor
    {
        private readonly IProductPostProcessor[] _processors;
        public IndexerDecorator(IProductPostProcessor[] processors)
        {
            _processors = processors;
        }

        public JObject Process(ProductData data)
        {
            foreach (var processor in _processors)
            {
                data.Product = processor.Process(data);
            }

            return data.Product;
        }
    }
}