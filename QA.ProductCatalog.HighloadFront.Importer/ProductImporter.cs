using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QA.Core;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.HighloadFront.Importer.DpcServiceReference;

namespace QA.ProductCatalog.HighloadFront.Importer
{
    public class ProductImporter
    {
        private readonly ILogger _logger;
        public Func<string, string, IDpcService> GetService { get; set; }
        public ProductManager Manager { get; set; }
        public HarvesterOptions Options { get; }

        public ProductImporter(IOptions<HarvesterOptions> optionsAccessor, Func<string, string, IDpcService> getService, ProductManager manager, ILogger logger)
        {
            _logger = logger;
            GetService = getService;
            Manager = manager;
            Options = optionsAccessor?.Value ?? new HarvesterOptions();
        }

        public async Task ImportAsync(ITaskExecutionContext executionContext, string language, string state)
        {
            if(executionContext.IsCancellationRequested)
            {
                executionContext.IsCancelled = true;

                return;
            }
            
            ThrowIfDisposed();
            var service = GetService(language, state);
            _logger.Info("Начинаем импорт данных. Запрашиваем список продуктов...");
            var ids = await service.GetAllProductIdAsync(0, int.MaxValue);
            _logger.Info($"Получен список продуктов. Количество: {ids.Length}");
            _logger.Info($"Бьем продукты на порции по {Options.ChunkSize}...");
            var chunks = ids.Chunk(Options.ChunkSize).ToArray();
            var index = 1;

            float progress = 0;

            foreach (var chunk in chunks)
            {
                if (executionContext.IsCancellationRequested)
                {
                    executionContext.IsCancelled = true;

                    return;
                }
           

                _logger.Info($"Запрашиваем порцию №{index}...");
                var dataTasks = chunk.Select(service.GetProductDataAsync);

                ProductData[] data;
                try
                {
                    data = await Task.WhenAll(dataTasks);
                }
                catch (Exception)
                {
                    string message = "Не удалось получить продукты от удаленного сервиса.";
                    _logger.Error(message);
                    executionContext.Message = message;
                    throw;
                }
                _logger.Info("Продукты получены.");

                _logger.Info("Начинаем разбор продуктов...");
                var products = data
                    .Select(j => new ProductPostProcessorData(JsonConvert.DeserializeObject<DpcResponse>(j.Product).Product, j.Created, j.Updated))
                    .ToArray();

                _logger.Info("Продукты разобраны");

                _logger.Info("Начинаем запись...");

                var result = await Manager.BulkCreateAsync(products, language, state);

                if (result.Succeeded)
                {
                    _logger.Info("Запись прошла успешно.");
                    index++;
                }
                else
                {
                    string message = $"Не удалось импортировать все продукты. {result}";
                    _logger.Error(message);
                    executionContext.Message = message;
                    throw result.GetException();
                }

                progress += (float)100 / chunks.Length;

                executionContext.SetProgress((byte)progress);
            }

            executionContext.Message = "Продукты проиндексированы";
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #region IDisposable Support
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;
            Manager.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}