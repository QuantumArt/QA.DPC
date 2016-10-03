﻿using QA.ProductCatalog.HighloadFront.Infrastructure;
using System;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class DateIndexer : IProductPostProcessor
    {
        private SonicElasticStoreOptions Options { get; set; }

        public DateIndexer(IOptions<SonicElasticStoreOptions> optionsAccessor)
        {
            Options = optionsAccessor?.Value ?? new SonicElasticStoreOptions();
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