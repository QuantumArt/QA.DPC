﻿using Newtonsoft.Json.Linq;
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

        public async Task<string> FindByIdAsync(ProductsOptions options, string language, string state)
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

        
        public async Task<string> SearchAsync(ProductsOptions options, string language, string state)
        {
            var store = StoreFactory.GetProductStore(language, state);
            var products = await store.SearchAsync(options, language, state);
            products = await _readProcessor.GetResult(products, options);
            return await Expand(store, options, language, state, products);
        }

        private async Task<string> Expand(IProductStore store, ProductsOptions options, string language, string state, string document)
        {
            var expanded = document;

            if (options.Expand != null && options.Expand.Any())
            {
                var ids = _readProcessor.GetExpandIds(document, options);
                if (ids.Any())
                {
                    var expandSource = await store.FindSourceByIdsAsync(ids, language, state);
                    expanded = _readProcessor.Expand(expanded, expandSource, options);
                }
            }

            return expanded;
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