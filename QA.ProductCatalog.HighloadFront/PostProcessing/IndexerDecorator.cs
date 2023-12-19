using System.Text.Json;
using System.Text.Json.Nodes;
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

        public void Process(ProductPostProcessorData data)
        {
            foreach (var processor in _processors)
            {
                processor.Process(data);
            }
        }
    }
}