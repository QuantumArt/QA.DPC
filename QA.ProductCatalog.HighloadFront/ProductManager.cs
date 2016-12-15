using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using QA.Core;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront
{
    public class ProductManager: IDisposable
    {
        private bool _disposed;
        private readonly IProductPostProcessor _productPostProcessor;

        protected IProductStore Store { get; }

        protected IEnumerable<IProductValidator> ProductValidators { get; } = new List<IProductValidator>();

        protected ILogger Logger { get; }

        protected SonicOptions Options { get; }

        public ProductManager(IProductStore store, IOptions<SonicOptions> optionsAccessor, ILogger logger, IProductPostProcessor productPostProcessor)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            Store = store;
            Logger = logger;
            Options = optionsAccessor?.Value ?? new SonicOptions();
            _productPostProcessor = productPostProcessor;
        }

        public Task<ElasticsearchResponse<string>> FindByIdAsync(string id, ProductsOptions options)
        {
            ThrowIfDisposed();
            var store = GetProductStreamStore();
            return store.FindByIdAsync(id, options);
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
            var result = await ValidateProductInternal(product);
            if (!result.Succeeded) return result;

             var data = new ProductPostProcessorData(product);

            if (_productPostProcessor != null)
                product = _productPostProcessor.Process(data);

            var tagsProduct = GetRegionTagsProduct(product, regionTags);

            if (tagsProduct != null)
            {
                var tagsResult = await Store.CreateAsync(product, language, state);

                if (!tagsResult.Succeeded)
                {
                    return tagsResult;
                }
            }
            else
            {
                tagsProduct = GetRegionTagsProduct(product, new RegionTag[] { new RegionTag() });

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
            var result = await ValidateProductsInternal(data.Select(d => d.Product).ToArray());
            if (!result.Succeeded) return result;
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


        private async Task<SonicResult> ValidateProductInternal(JObject product)
        {
            var errors = new List<SonicError>();
            foreach (var v in ProductValidators)
            {
                var result = await v.ValidateAsync(this, product);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }
            if (errors.Count <= 0) return SonicResult.Success;
            Logger.Info($"Product {GetProductId(product)} " +
                              $"validation failed: {string.Join(";", errors.Select(e => e.Code))}.");

            return SonicResult.Failed(errors.ToArray());
        }

        private async Task<SonicResult> ValidateProductsInternal(IList<JObject> products)
        {
            var resultsTasks = products.SelectMany(p => ProductValidators.Select(pv => pv.ValidateAsync(this, p)));
            var results = await Task.WhenAll(resultsTasks);
            var errors = results.Where(r => !r.Succeeded).SelectMany(r => r.Errors).ToList();

            if (!errors.Any()) return SonicResult.Success;
            Logger.Info("Products bulk validation failed.");
            return SonicResult.Failed(errors.ToArray());
        }

        public string GetProductId(JObject product)
        {
            return Store.GetId(product);
        }

      

        public Task<IList<JObject>> SearchAsync(string query, ProductsOptions options = null)
        {
            ThrowIfDisposed();
            var store = GetProductSearchStore();

            return store.SearchAsync(query, options ?? Options.Product);
        }

        public Task<Stream> SearchStreamAsync(string query, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            var store = GetProductSearchStore();

            return store.SearchStreamAsync(query, options ?? Options.Product, language, state);
        }

        public Task<IList<JObject>> GetProductsInTypeAsync(string type, ProductsOptions options)
        {
            ThrowIfDisposed();
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var store = GetProductTypeStore();

            return store.GetProductsInTypeAsync(type, options ?? Options.Product);
        }

        public Task<Stream> GetProductsInTypeStream(string type, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var store = GetProductTypeStore();

            return store.GetProductsInTypeStreamAsync(type, options ?? Options.Product, language, state);
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

            var tagsProduct = GetRegionTagsProduct(product, new RegionTag[] { new RegionTag() });

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