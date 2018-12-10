using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.PostProcessing
{
    public class DateIndexer : IProductPostProcessor
    {
        private SonicElasticStoreOptions Options { get; set; }

        public DateIndexer(SonicElasticStoreOptions options)
        {
            Options = options;
        }

        public JObject Process(ProductPostProcessorData data)
        {
            var product = data.Product;

            if (!string.IsNullOrEmpty(Options.CreationDateField))
            {                
                product.Add(new JProperty(Options.CreationDateField, data.Updated));
            }

            return product;
        }
    }
}