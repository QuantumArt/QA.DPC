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

        public async Task<SonicResult> BulkCreateAsync(ProductPostProcessorData[] data, string language, string state, Dictionary<string, IProductStore> stores, string index)
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

                return await store.BulkCreateAsync(products, language, state, index);              
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

        private IProductStore GetProductStore(string language, string state, Dictionary<string, IProductStore> stores)
        {
            var version = StoreFactory.GetProductStoreVersion(language, state);

            if (stores.TryGetValue(version, out var store))
            {
                return store;
            }

            throw StoreFactory.ElasticVersionNotSupported(version);
        }

        public async Task<List<string>> GetIndexesToDeleteAsync(string language, string state, Dictionary<string, IProductStore> stores, string alias)
        {
            IProductStore store = GetProductStore(language, state, stores);
            string indexes = await store.GetIndiceByName(language, state);
            return store.RetrieveIndexesFromIndicesResponse(indexes, alias);
        }

        public async Task<string> CreateVersionedIndexAsync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            return await GetProductStore(language, state, stores).CreateVersionedIndexAsync(language, state);
        }

        public async Task AddIndexToAliasAsync(string language, string state, Dictionary<string, IProductStore> stores, string newIndex, string alias, string[] oldIndexes)
        {
            await GetProductStore(language, state, stores).AddIndexToAliasAsync(language, state, newIndex, oldIndexes, alias);
        }

        public async Task DeleteIndexByNameAsync(string language, string state, Dictionary<string, IProductStore> stores, string index)
        {
            await GetProductStore(language, state, stores).DeleteIndexByNameAsync(language, state, index);
        }

        public async Task<string[]> GetIndexesInAliasAsync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            return await GetProductStore(language, state, stores).GetIndexInAliasAsync(language, state);
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