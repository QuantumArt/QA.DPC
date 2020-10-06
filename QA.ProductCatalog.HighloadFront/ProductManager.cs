using System;
using System.Collections.Generic;
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

        protected IProductStoreFactory StoreFactory { get; }

        public ProductManager(IProductStoreFactory storeFactory, IProductPostProcessor productPostProcessor)
        {
            StoreFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
            _productPostProcessor = productPostProcessor;
        }

        public Task<string> FindByIdAsync(ProductsOptions options, string language, string state)
        {            
            return StoreFactory.GetProductStore(language, state).FindByIdAsync(options, language, state);
        }

        public async Task<SonicResult> CreateAsync(JObject product, RegionTag[] regionTags, string language, string state)
        {
             var data = new ProductPostProcessorData(product);

            if (_productPostProcessor != null)
                product = _productPostProcessor.Process(data);

            var tagsProduct = GetRegionTagsProduct(product, regionTags, language, state);

            if (tagsProduct != null)
            {
                var tagsResult = await StoreFactory.GetProductStore(language, state).CreateAsync(tagsProduct, language, state);

                if (!tagsResult.Succeeded)
                {
                    return tagsResult;
                }
            }
            else
            {
                tagsProduct = GetRegionTagsProduct(product, new[] { new RegionTag() }, language, state);

                if (await StoreFactory.GetProductStore(language, state).Exists(tagsProduct, language, state))
                {
                    var deleteTagsResult = await StoreFactory.GetProductStore(language, state).DeleteAsync(tagsProduct, language, state);

                    if (!deleteTagsResult.Succeeded)
                    {
                        return deleteTagsResult;
                    }
                }
            }

            return await StoreFactory.GetProductStore(language, state).CreateAsync(product, language, state);
        }

        public async Task<SonicResult> BulkCreateAsync(ProductPostProcessorData[] data, string language, string state, Dictionary<string, IProductStore> stores)
        {
            var filteredData = data.Where(n => n != null).ToArray();
            if (_productPostProcessor != null)
            {
                foreach (var d in filteredData)
                {
                    d.Product = _productPostProcessor.Process(d);
                }
            }

            var version = StoreFactory.GetProductStoreVersion(language, state);
            if (stores.TryGetValue(version, out var store))
            {
                var regionTagProducts = filteredData.Select(d => GetRegionTagsProduct(int.Parse(store.GetId(d.Product)), d.RegionTags))
                    .Where(d => d != null);
                var products = filteredData.Select(d => d.Product).Concat(regionTagProducts);

                return await store.BulkCreateAsync(products, language, state);              
            }
            throw StoreFactory.ElasticVersionNotSupported(version);
        }


        public string GetProductId(JObject product,string language, string state)
        {
            return StoreFactory.GetProductStore(language, state).GetId(product);
        }

        
        public Task<string> SearchAsync(ProductsOptions options, string language, string state)
        {
            return StoreFactory.GetProductStore(language, state).SearchAsync(options, language, state);
        }

      
        public Task<SonicResult> DeleteAllASync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            var version = StoreFactory.GetProductStoreVersion(language, state);
            if (stores.TryGetValue(version, out var store))
            {
                return store.ResetAsync(language, state);
            }
            throw StoreFactory.ElasticVersionNotSupported(version);
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
        {

            var tagsProduct = GetRegionTagsProduct(product, new[] { new RegionTag() }, language, state);

            if (await StoreFactory.GetProductStore(language, state).Exists(tagsProduct, language, state))
            {
                var deleteTagsResult = await StoreFactory.GetProductStore(language, state).DeleteAsync(tagsProduct, language, state);

                if (!deleteTagsResult.Succeeded)
                {
                    return deleteTagsResult;
                }
            }

            return await StoreFactory.GetProductStore(language, state).DeleteAsync(product, language, state);
        }

        JObject GetRegionTagsProduct(JObject product, RegionTag[] regionTags, string language, string state)
        {
            var productId = int.Parse(GetProductId(product, language, state));
            return GetRegionTagsProduct(productId, regionTags);
        }
        
        JObject GetRegionTagsProduct(int productId, RegionTag[] regionTags)
        {
            if (regionTags == null || !regionTags.Any())
            {
                return null;
            }

            var tags = JObject.FromObject(new
            {
                Id = -productId,
                ProductId = productId,
                Type = "RegionTags",
                RegionTags = regionTags
            });

            return tags;
        }
    }
}