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

        public Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(string id, ProductsOptions options)
        {
            ThrowIfDisposed();
            var store = GetProductStreamStore();
            return store.FindStreamByIdAsync(id, options);
        }

        public async Task<SonicResult> CreateAsync(JObject product)
        {
            ThrowIfDisposed();
            var result = await ValidateProductInternal(product);
            if (!result.Succeeded) return result;

            if (_productPostProcessor != null)
                product = _productPostProcessor.Process(product);

            return await Store.CreateAsync(product);
        }

        public async Task<SonicResult> BulkCreateAsync(JObject[] products)
        {
            ThrowIfDisposed();
            var result = await ValidateProductsInternal(products);
            if (!result.Succeeded) return result;
            var store = GetProductBulkStore();

            IEnumerable<JObject> productsToSave;

            if (_productPostProcessor != null)
                productsToSave = products.Select(x => _productPostProcessor.Process(x));
            else
                productsToSave = products;

            return await store.BulkCreateAsync(productsToSave);
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

        public Task<Stream> SearchStreamAsync(string query, ProductsOptions options = null)
        {
            ThrowIfDisposed();
            var store = GetProductSearchStore();

            return store.SearchStreamAsync(query, options ?? Options.Product);
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

        public Task<Stream> GetProductsInTypeStream(string type, ProductsOptions options)
        {
            ThrowIfDisposed();
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var store = GetProductTypeStore();

            return store.GetProductsInTypeStreamAsync(type, options ?? Options.Product);
        }



        public Task<SonicResult> DeleteAllASync()
        {
            ThrowIfDisposed();
            return Store.ResetAsync();
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

        public Task<SonicResult> DeleteAsync(JObject product)
        {
            ThrowIfDisposed();
            return Store.DeleteAsync(product);
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