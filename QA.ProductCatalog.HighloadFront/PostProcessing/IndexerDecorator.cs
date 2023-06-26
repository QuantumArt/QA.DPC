using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class IndexerDecorator : IProductWritePostProcessor
    {
        private readonly IProductWritePostProcessor[] _processors;
        public IndexerDecorator(IProductWritePostProcessor[] processors)
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