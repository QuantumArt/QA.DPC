using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class IndexerDecorator : IProductPostProcessor
    {
        private readonly IProductPostProcessor[] _processors;
        public IndexerDecorator(IProductPostProcessor[] processors)
        {
            _processors = processors;
        }

        public JObject Process(ProductPostProcessorData data)
        {
            foreach (var processor in _processors)
            {
                data.Product = processor.Process(data);
            }

            return data.Product;
        }
    }
}