using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.Logger;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager: IDisposable
    {
        private bool _disposed;
        private readonly IProductPostProcessor _productPostProcessor;

        protected IProductStore Store { get; }

        protected ILogger Logger { get; }

        protected SonicOptions Options { get; }

        public ProductManager(IProductStore store, IOptions<SonicOptions> optionsAccessor, ILogger logger, IProductPostProcessor productPostProcessor)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Logger = logger;
            Options = optionsAccessor?.Value ?? new SonicOptions();
            _productPostProcessor = productPostProcessor;
        }

        public Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(string id, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            var store = GetProductStreamStore();
            return store.FindStreamByIdAsync(id, options, language, state);
        }

        public async Task<SonicResult> CreateAsync(JObject product, RegionTag[] regionTags, string language, string state)
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();
            var store = GetProductBulkStore();

            if (_productPostProcessor != null)
            {
                foreach (var d in data)
                {
                    d.Product = _productPostProcessor.Process(d);
                }
            }

            var regionTagProducts = data.Select(d => GetRegionTagsProduct(d.Product, d.RegionTags)).Where(d => d != null);
            var products = data.Select(d => d.Product).Concat(regionTagProducts);

            return await store.BulkCreateAsync(products, language, state);
        }


        public string GetProductId(JObject product)
        {
            return Store.GetId(product);
        }

      
        public Task<Stream> SearchStreamAsync(ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            var store = GetProductSearchStore();

            return store.SearchStreamAsync(options ?? Options.Product, language, state);
        }

        public Task<Stream> GetProductsInTypeStream(ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            if (options?.Type == null)
            {
                throw new ArgumentNullException("type");
            }
            var store = GetProductTypeStore();

            return store.GetProductsInTypeStreamAsync(options, language, state);
        }


        public Task<SonicResult> DeleteAllASync(string language, string state)
        {
            ThrowIfDisposed();
            return Store.ResetAsync(language, state);
        }

        private IProductTypeStore GetProductTypeStore()
        {
            var cast = Store as IProductTypeStore;
            if (cast == null)
            {
                throw new NotSupportedException($"Store does not implement {nameof(IProductTypeStore)}.");
            }
            return cast;
        }

        private IProductSearchStore GetProductSearchStore()
        {
            var cast = Store as IProductSearchStore;
            if (cast == null)
            {
                throw new NotSupportedException($"Store does not implement {nameof(IProductSearchStore)}.");
            }
            return cast;
        }

        private IProductBulkStore GetProductBulkStore()
        {
            var cast = Store as IProductBulkStore;
            if (cast == null)
            {
                throw new NotSupportedException($"Store does not implement {nameof(IProductBulkStore)}.");
            }
            return cast;
        }

        private IProductStreamStore GetProductStreamStore()
        {
            var cast = Store as IProductStreamStore;
            if (cast == null)
            {
                throw new NotSupportedException($"Store does not implement {nameof(IProductBulkStore)}.");
            }
            return cast;
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
        {
            ThrowIfDisposed();

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


        #region IDisposable Support

        public void Dispose()
        {
            Logger.Debug("Disposing manager");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            Store.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
        #endregion
    }
}