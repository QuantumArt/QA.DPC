using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager
    {
        private readonly IProductStoreFactory _storeFactory;
        private readonly ICacheProvider _cacheProvider;
        private readonly IProductReadPostProcessor _productReadPostProcessor;
        private readonly IProductWritePostProcessor _productWritePostProcessor;
        private readonly IProductReadExpandPostProcessor _productReadExpandPostProcessor;
        private readonly IProductWriteExpandPostProcessor _productWriteExpandPostProcessor;
        private readonly HashProcessor _hashProcessor;

        public ProductManager(
            IProductStoreFactory storeFactory,
            ICacheProvider cacheProvider,
            IProductReadPostProcessor productReadPostProcessor,
            IProductWritePostProcessor productWritePostProcessor,
            IProductReadExpandPostProcessor productReadExpandPostProcessor,
            IProductWriteExpandPostProcessor productWriteExpandPostProcessor,
            HashProcessor hashProcessor)
        {
            _storeFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
            _cacheProvider = cacheProvider;
            _productReadPostProcessor = productReadPostProcessor;
            _productWritePostProcessor = productWritePostProcessor;
            _productReadExpandPostProcessor = productReadExpandPostProcessor;
            _productWriteExpandPostProcessor = productWriteExpandPostProcessor;
            _hashProcessor = hashProcessor;
        }

        public async Task<JToken> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var store = await _storeFactory.GetProductStore(language, state);
            var product = JObject.Parse(await store.FindByIdAsync(options, language, state));
            await Expand(store, product, options, language, state);
            return product;
        }

        public async Task<SonicResult> CreateAsync(JObject product, RegionTag[] regionTags, string language, string state)
        {
            var data = new ProductPostProcessorData(product);

            if (_productWritePostProcessor != null)
            {
                product = _productWritePostProcessor.Process(data);
            }

            var store = await _storeFactory.GetProductStore(language, state);
            var tagsProduct = await GetRegionTagsProduct(product, regionTags, language, state);

            if (tagsProduct != null)
            {
                var tagsResult = await store.CreateAsync(tagsProduct, language, state);

                if (!tagsResult.Succeeded)
                {
                    return tagsResult;
                }
            }
            else
            {
                tagsProduct = await GetRegionTagsProduct(product, new[] { new RegionTag() }, language, state);

                if (await store.Exists(tagsProduct, language, state))
                {
                    var deleteTagsResult = await store.DeleteAsync(tagsProduct, language, state);

                    if (!deleteTagsResult.Succeeded)
                    {
                        return deleteTagsResult;
                    }
                }
            }

            return await store.CreateAsync(product, language, state);
        }

        public async Task<SonicResult> BulkCreateAsync(ProductPostProcessorData[] data, string language, string state, Dictionary<string, IProductStore> stores, string index)
        {
            var filteredData = data.Where(n => n != null).ToArray();
            if (_productWritePostProcessor != null)
            {
                foreach (var d in filteredData)
                {
                    d.Product = _productWritePostProcessor.Process(d);
                }
            }

            var version = await _storeFactory.GetProductStoreVersion(language, state);
            if (stores.TryGetValue(version, out var store))
            {
                var regionTagProducts = filteredData.Select(d => GetRegionTagsProduct(int.Parse(store.GetId(d.Product)), d.RegionTags))
                    .Where(d => d != null);
                var products = filteredData.Select(d => d.Product).Concat(regionTagProducts);

                return await store.BulkCreateAsync(products, language, state, index);              
            }
            throw _storeFactory.ElasticVersionNotSupported(version);
        }

        public async Task<string> GetProductId(JObject product,string language, string state)
        {
            var store = await _storeFactory.GetProductStore(language, state);
            return store.GetId(product);
        }
        
        public async Task<JArray> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default)
        {
            var store = await _storeFactory.GetProductStore(language, state);
            var productsRawData = await store.SearchAsync(options, language, state, cancellationToken);
            var products = await _productReadPostProcessor.ReadSourceNodes(productsRawData, options);
            
            if (products.Any())
            {
                await Expand(store, products, options, language, state);
            }

            return products;
        }

        public async Task<List<string>> GetIndexesToDeleteAsync(string language, string state, Dictionary<string, IProductStore> stores, string alias)
        {
            var store = await GetProductStore(language, state, stores);
            var indices = await store.GetIndicesByName(language, state);
            return store.RetrieveIndexNamesFromIndicesResponse(indices, alias);
        }

        public async Task<string> CreateVersionedIndexAsync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            var store = await GetProductStore(language, state, stores);
            return await store.CreateVersionedIndexAsync(language, state);
        }

        public async Task ReplaceIndexesInAliasAsync(string language, string state, Dictionary<string, IProductStore> stores, string newIndex, string alias, string[] oldIndexes)
        {
            var store = await GetProductStore(language, state, stores);
            await store.ReplaceIndexesInAliasAsync(language, state, newIndex, oldIndexes, alias);
        }

        public async Task DeleteIndexByNameAsync(string language, string state, Dictionary<string, IProductStore> stores, string index)
        {
            var store = await GetProductStore(language, state, stores);
            await store.DeleteIndexByNameAsync(language, state, index);
        }

        public async Task DeleteIndexesByNamesAsync(string language, string state, Dictionary<string, IProductStore> stores, List<string> indexNames)
        {
            var store = await GetProductStore(language, state, stores);

            foreach (string indexName in indexNames)
            {
                await store.DeleteIndexByNameAsync(language, state, indexName);
            }
        }

        public async Task<string[]> GetIndexesInAliasAsync(string language, string state, Dictionary<string, IProductStore> stores)
        {
            var store = await GetProductStore(language, state, stores);
            return await store.GetIndexInAliasAsync(language, state);
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
        {
            var tagsProduct = await GetRegionTagsProduct(product, new[] { new RegionTag() }, language, state);
            var store = await _storeFactory.GetProductStore(language, state);
            if (await store.Exists(tagsProduct, language, state))
            {
                var deleteTagsResult = await store.DeleteAsync(tagsProduct, language, state);

                if (!deleteTagsResult.Succeeded)
                {
                    return deleteTagsResult;
                }
            }

            return await store.DeleteAsync(product, language, state);
        }

        private async Task<IProductStore> GetProductStore(string language, string state, Dictionary<string, IProductStore> stores)
        {
            var version = await _storeFactory.GetProductStoreVersion(language, state);

            if (stores.TryGetValue(version, out var store))
            {
                return store;
            }

            throw _storeFactory.ElasticVersionNotSupported(version);
        }

        private async Task<JObject> GetRegionTagsProduct(JObject product, RegionTag[] regionTags, string language, string state)
        {
            var productId = int.Parse(await GetProductId(product, language, state));
            return GetRegionTagsProduct(productId, regionTags);
        }

        private JObject GetRegionTagsProduct(int productId, RegionTag[] regionTags)
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

        private async Task Expand(IProductStore store, JToken input, ProductsOptionsBase options, string language, string state)
        {
            if (options.Expand == null)
            {
                return;
            }

            foreach (var expandOptions in options.Expand)
            {
                var expandIds = _productReadExpandPostProcessor.GetExpandIdsWithVerification(input, expandOptions);
                if (!expandIds.Any())
                {
                    continue;
                }

                expandOptions.FilterByIds(expandIds);

                var extraNodes = await GetExtraNodesTask(store, expandOptions, language, state);
                if (!extraNodes.Any())
                {
                    continue;
                }

                await Expand(store, extraNodes, expandOptions, language, state);
                _productWriteExpandPostProcessor.WriteExtraNodes(input, extraNodes, expandOptions);
            }
        }

        private Task<JArray> GetExtraNodesTask(IProductStore store, ProductsOptionsExpand expandOptions, string language, string state)
        {
            var cachePeriod = TimeSpan.FromSeconds(decimal.ToDouble(expandOptions.CacheForSeconds));

            var extraNodesSearchFunc = async () =>
            {
                var extraNodesRawData = await store.SearchAsync(expandOptions, language, state);
                return await _productReadPostProcessor.ReadSourceNodes(extraNodesRawData, expandOptions);
            };

            if (cachePeriod == TimeSpan.Zero)
            {
                return extraNodesSearchFunc();
            }

            var expandKeyMainPart = _hashProcessor.ComputeHash(JsonConvert.SerializeObject(expandOptions));
            var expandCacheKey = $"expand:{language}-{state}:{expandKeyMainPart}";
            return _cacheProvider.GetOrAddAsync(
                expandCacheKey,
                Array.Empty<string>(),
                cachePeriod,
                async () => await extraNodesSearchFunc());
        }
    }
}