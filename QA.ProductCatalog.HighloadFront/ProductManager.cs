using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager
    {
        private readonly IProductWritePostProcessor _productPostProcessor;
        private readonly IProductReadPostProcessor _readProcessor;

        protected IProductStoreFactory StoreFactory { get; }

        public ProductManager(IProductStoreFactory storeFactory, IProductWritePostProcessor productPostProcessor, IProductReadPostProcessor readProcessor)
        {
            StoreFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
            _productPostProcessor = productPostProcessor;
            _readProcessor = readProcessor;
        }

        public async Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var store = StoreFactory.GetProductStore(language, state);
            var product = await store.FindByIdAsync(options, language, state);
            return await Expand(store, options, language, state, product);
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

        
        public async Task<string> SearchAsync(ProductsOptionsBase options, string language, string state)
        {
            var store = StoreFactory.GetProductStore(language, state);
            var products = await store.SearchAsync(options, language, state);
            products = await _readProcessor.GetResult(products, options);
            return await Expand(store, options, language, state, products);
        }

        private async Task<string> Expand(IProductStore store, ProductsOptionsBase options, string language, string state, string document)
        {
            var expanded = document;

            if (options.Expand != null)
            {
                foreach(var expandOptions in options.Expand)
                {
                    var ids = _readProcessor.GetExpandIds(document, expandOptions);

                    if (ids.Any())
                    {
                        expandOptions.FilterByIds(ids);
                        var expandSource = await store.SearchAsync(expandOptions, language, state);
                        //expandSource = await Expand(store, expandOptions, language, state, expandSource);

                        var xx = _readProcessor.GetExpand(expandSource);
                        var yy = await Expand(store, expandOptions, language, state, xx.ToString());


                        expanded = _readProcessor.Expand(expanded, yy, expandOptions);
                    }
                }
            }

            return expanded;
        }


        //private async Task<string> ExpandDeprecate(IProductStore store, ProductsOptionsBase options, string language, string state, string document)
        //{

        //    var expanded = document;

        //    if (options.Expand != null && options.Expand.Any())
        //    {
        //        var ids = _readProcessor.GetExpandIds(document, options);
        //        if (ids.Any())
        //        {
        //            var expandSource = await store.FindSourceByIdsAsync(ids, language, state);
        //            expanded = _readProcessor.Expand(expanded, expandSource, options);
        //        }
        //    }

        //    return expanded;
        //}

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
            string indices = await store.GetIndicesByName(language, state);
            return store.RetrieveIndexNamesFromIndicesResponse(indices, alias);
        }

        public async Task<string> CreateVersionedIndexAsync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            return await GetProductStore(language, state, stores).CreateVersionedIndexAsync(language, state);
        }

        public async Task ReplaceIndexesInAliasAsync(string language, string state, Dictionary<string, IProductStore> stores, string newIndex, string alias, string[] oldIndexes)
        {
            await GetProductStore(language, state, stores).ReplaceIndexesInAliasAsync(language, state, newIndex, oldIndexes, alias);
        }

        public async Task DeleteIndexByNameAsync(string language, string state, Dictionary<string, IProductStore> stores, string index)
        {
            await GetProductStore(language, state, stores).DeleteIndexByNameAsync(language, state, index);
        }

        public async Task DeleteIndexesByNamesAsync(string language, string state, Dictionary<string, IProductStore> stores, List<string> indexNames)
        {
            var store = GetProductStore(language, state, stores);

            foreach (string indexName in indexNames)
            {
                await store.DeleteIndexByNameAsync(language, state, indexName);
            }
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