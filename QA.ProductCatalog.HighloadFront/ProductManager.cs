using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager
    {
        private readonly IProductPostProcessor _productPostProcessor;

        protected IProductStore Store { get; }

        public ProductManager(IProductStore store, IProductPostProcessor productPostProcessor)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            _productPostProcessor = productPostProcessor;
        }

        public Task<string> FindByIdAsync(ProductsOptions options, string language, string state)
        {
            return Store.FindByIdAsync(options, language, state);
        }

        public async Task<SonicResult> CreateAsync(JObject product, RegionTag[] regionTags, string language, string state)
        {
             var data = new ProductPostProcessorData(product);

            if (_productPostProcessor != null)
                product = _productPostProcessor.Process(data);

            var tagsProduct = GetRegionTagsProduct(product, regionTags);

            if (tagsProduct != null)
            {
                var tagsResult = await Store.CreateAsync(tagsProduct, language, state);

                if (!tagsResult.Succeeded)
                {
                    return tagsResult;
                }
            }
            else
            {
                tagsProduct = GetRegionTagsProduct(product, new[] { new RegionTag() });

                if (await Store.Exists(tagsProduct, language, state))
                {
                    var deleteTagsResult = await Store.DeleteAsync(tagsProduct, language, state);

                    if (!deleteTagsResult.Succeeded)
                    {
                        return deleteTagsResult;
                    }
                }
            }

            return await Store.CreateAsync(product, language, state);
        }

        public async Task<SonicResult> BulkCreateAsync(ProductPostProcessorData[] data, string language, string state)
        {

            if (_productPostProcessor != null)
            {
                foreach (var d in data)
                {
                    d.Product = _productPostProcessor.Process(d);
                }
            }

            var regionTagProducts = data.Select(d => GetRegionTagsProduct(d.Product, d.RegionTags)).Where(d => d != null);
            var products = data.Select(d => d.Product).Concat(regionTagProducts);

            return await Store.BulkCreateAsync(products, language, state);
        }


        public string GetProductId(JObject product)
        {
            return Store.GetId(product);
        }

        
        public Task<string> SearchAsync(ProductsOptions options, string language, string state)
        {
            return Store.SearchAsync(options, language, state);
        }

      
        public Task<SonicResult> DeleteAllASync(string language, string state)
        {
            return Store.ResetAsync(language, state);
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
        {

            var tagsProduct = GetRegionTagsProduct(product, new[] { new RegionTag() });

            if (await Store.Exists(tagsProduct, language, state))
            {
                var deleteTagsResult = await Store.DeleteAsync(tagsProduct, language, state);

                if (!deleteTagsResult.Succeeded)
                {
                    return deleteTagsResult;
                }
            }

            return await Store.DeleteAsync(product, language, state);
        }

        JObject GetRegionTagsProduct(JObject product, RegionTag[] regionTags)
        {
            if (regionTags == null || !regionTags.Any())
            {
                return null;
            }
            else
            {
                var id = int.Parse(GetProductId(product));

                var tags = JObject.FromObject(new
                {
                    Id = -id,
                    ProductId = id,
                    Type = "RegionTags",
                    RegionTags = regionTags
                });

                return tags;
            }
        }
    }
}