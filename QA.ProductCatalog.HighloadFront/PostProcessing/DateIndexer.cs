using System.Text.Json.Nodes;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class DateIndexer : IProductWritePostProcessor
    {
        private SonicElasticStoreOptions Options { get; set; }

        public DateIndexer(SonicElasticStoreOptions options)
        {
            Options = options;
        }

        public void Process(ProductPostProcessorData data)
        {
            if (!string.IsNullOrEmpty(Options.CreationDateField))
            {
                var value = JsonValue.Create(data.Updated.ToString(Options.DateFormat));
                data.Product.Add(new (Options.CreationDateField, value));
            }
        }
    }
}