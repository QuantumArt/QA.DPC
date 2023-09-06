﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.HighloadFront.Constants;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;
using N = Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager
    {
        private readonly IProductStoreFactory _storeFactory;
        private readonly ICacheProvider _cacheProvider;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IProductReadPostProcessor _productReadPostProcessor;
        private readonly IProductWritePostProcessor _productWritePostProcessor;
        private readonly IProductReadExpandPostProcessor _productReadExpandPostProcessor;
        private readonly IProductWriteExpandPostProcessor _productWriteExpandPostProcessor;
        private readonly HashProcessor _hashProcessor;
        private string _customerCode;

        public ProductManager(
            IProductStoreFactory storeFactory,
            ICacheProvider cacheProvider,
            IConnectionProvider connectionProvider,
            IProductReadPostProcessor productReadPostProcessor,
            IProductWritePostProcessor productWritePostProcessor,
            IProductReadExpandPostProcessor productReadExpandPostProcessor,
            IProductWriteExpandPostProcessor productWriteExpandPostProcessor,
            HashProcessor hashProcessor)
        {
            _storeFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
            _cacheProvider = cacheProvider;
            _connectionProvider = connectionProvider;
            _productReadPostProcessor = productReadPostProcessor;
            _productWritePostProcessor = productWritePostProcessor;
            _productReadExpandPostProcessor = productReadExpandPostProcessor;
            _productWriteExpandPostProcessor = productWriteExpandPostProcessor;
            _hashProcessor = hashProcessor;
        }

        public void SetCustomerCode(string code)
        {
            _customerCode = code;
        }

        public string CustomerCode => _customerCode ?? _connectionProvider.GetCustomer().CustomerCode;

        public async Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var store = await _storeFactory.GetProductStore(CustomerCode, language, state);
            var product = await store.FindByIdAsync(options, language, state);
            return await ExpandProduct(product, options, language, state, store);
        }

        public async Task<SonicResult> CreateAsync(N.JObject product, RegionTag[] regionTags, string language, string state)
        {
            var data = new ProductPostProcessorData(product);

            if (_productWritePostProcessor != null)
            {
                product = _productWritePostProcessor.Process(data);
            }

            var store = await _storeFactory.GetProductStore(CustomerCode, language, state);
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

            var version = await _storeFactory.GetProductStoreVersion(CustomerCode, language, state);
            if (stores.TryGetValue(version, out var store))
            {
                var regionTagProducts = filteredData.Select(d => GetRegionTagsProduct(int.Parse(store.GetId(d.Product)), d.RegionTags))
                    .Where(d => d != null);
                var products = filteredData.Select(d => d.Product).Concat(regionTagProducts);

                return await store.BulkCreateAsync(products, language, state, index);              
            }
            throw _storeFactory.ElasticVersionNotSupported(version);
        }

        public async Task<string> GetProductId(N.JObject product,string language, string state)
        {
            var store = await _storeFactory.GetProductStore(CustomerCode, language, state);
            return store.GetId(product);
        }
        
        public async Task<string> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default)
        {
            var store = await _storeFactory.GetProductStore(CustomerCode, language, state);
            var productsRawData = await store.SearchAsync(options, language, state, cancellationToken);
            var products = await _productReadPostProcessor.ReadSourceNodes(productsRawData, options);
            return await ExpandProducts(products, options, language, state, store);
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

        public async Task<SonicResult> DeleteAsync(N.JObject product, string language, string state)
        {
            var tagsProduct = await GetRegionTagsProduct(product, new[] { new RegionTag() }, language, state);
            var store = await _storeFactory.GetProductStore(CustomerCode, language, state);
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
            var version = await _storeFactory.GetProductStoreVersion(CustomerCode, language, state);

            if (stores.TryGetValue(version, out var store))
            {
                return store;
            }

            throw _storeFactory.ElasticVersionNotSupported(version);
        }

        private async Task<N.JObject> GetRegionTagsProduct(N.JObject product, RegionTag[] regionTags, string language, string state)
        {
            var productId = int.Parse(await GetProductId(product, language, state));
            return GetRegionTagsProduct(productId, regionTags);
        }

        private N.JObject GetRegionTagsProduct(int productId, RegionTag[] regionTags)
        {
            if (regionTags == null || !regionTags.Any())
            {
                return null;
            }

            var tags = N.JObject.FromObject(new
            {
                Id = -productId,
                ProductId = productId,
                Type = "RegionTags",
                RegionTags = regionTags
            });

            return tags;
        }

        private async Task Expand(IProductStore store, JsonNode input, ProductsOptionsBase options, string language, string state)
        {
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
                
                if (expandOptions.Expand != null)
                {
                    await Expand(store, extraNodes, expandOptions, language, state);
                }
                
                _productWriteExpandPostProcessor.WriteExtraNodes(input, extraNodes, expandOptions);
            }
        }

        private async Task<JsonArray> GetExtraNodesTask(IProductStore store, ProductsOptionsExpand expandOptions, string language, string state)
        {
            var cachePeriod = TimeSpan.FromSeconds(decimal.ToDouble(expandOptions.CacheForSeconds));

            var extraNodesSearchFunc = async () =>
            {
                var extraNodesRawData = await store.SearchAsync(expandOptions, language, state);
                return await _productReadPostProcessor.ReadSourceNodes(extraNodesRawData, expandOptions);
            };

            if (cachePeriod == TimeSpan.Zero)
            {
                return JsonNode.Parse(await extraNodesSearchFunc()).AsArray();
            }

            var expandKeyMainPart = _hashProcessor.ComputeHash(JsonSerializer.Serialize(expandOptions));
            var expandCacheKey = $"expand:{language}-{state}:{expandKeyMainPart}";
            return JsonNode.Parse(await _cacheProvider.GetOrAddAsync(
                expandCacheKey,
                Array.Empty<string>(),
                cachePeriod,
                async () => await extraNodesSearchFunc())).AsArray();
        }
        
        private async Task<string> ExpandProducts(string products, ProductsOptionsBase options, string language, string state, IProductStore store)
        {
            if (!string.IsNullOrEmpty(products) && products != HighloadCommonConstants.EmptyArray && options.Expand != null)
            {
                var jProducts = JsonNode.Parse(products).AsArray();
                await Expand(store, jProducts, options, language, state);
                products = jProducts.ToJsonString(PostProcessHelper.GetSerializerOptions());
            }
            return products;
        }
        
        private async Task<string> ExpandProduct(string product, ProductsOptionsBase options, string language, string state, IProductStore store)
        {
            if (!string.IsNullOrEmpty(product) && options.Expand != null)
            {
                var jProduct = JsonNode.Parse(product);
                await Expand(store, jProduct, options, language, state);
                product = jProduct.ToJsonString(PostProcessHelper.GetSerializerOptions());
            }
            return product;
        }

    }
}