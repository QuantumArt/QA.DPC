using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Utils;

namespace QA.ProductCatalog.HighloadFront.Importer
{
    public class ProductImporter
    {
        private readonly ILogger _logger;

        private readonly IElasticConfiguration _configuration;

        private readonly ProductManager _manager;

        private readonly HarvesterOptions _options;

        public ProductImporter(IOptions<HarvesterOptions> optionsAccessor, IElasticConfiguration configuration, ProductManager manager, ILogger logger)
        {
            _logger = logger;
            _manager = manager;
            _configuration = configuration;
            _options = optionsAccessor?.Value ?? new HarvesterOptions();
        }

        public async Task ImportAsync(ITaskExecutionContext executionContext, string language, string state)
        {
            if(executionContext.IsCancellationRequested)
            {
                executionContext.IsCancelled = true;

                return;
            }
            
            ThrowIfDisposed();
            var url = _configuration.GetReindexUrl(language, state);
            _logger.Info("Начинаем импорт данных. Запрашиваем список продуктов...");
            var ids = await GetIds(url);
            _logger.Info($"Получен список продуктов. Количество: {ids.Length}");
            _logger.Info($"Бьем продукты на порции по {_options.ChunkSize}...");
            var chunks = ids.Chunk(_options.ChunkSize).ToArray();
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
                var dataTasks = chunk.Select(n => GetProductById(url, n));

                ProductPostProcessorData[] data;
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


                _logger.Info("Начинаем запись...");

                var result = await _manager.BulkCreateAsync(data, language, state);

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

        public async Task<Tuple<string, DateTime>> GetContent(string url)
        {
            DateTime modified;
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                var response = await client.GetAsync(url);
                modified = response.Content.Headers.LastModified?.DateTime ?? DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            return new Tuple<string, DateTime>(result, modified);
        }

        public async Task<int[]> GetIds(string reindexUrl)
        {
            var result = await GetContent(reindexUrl);
            var arr = JsonConvert.DeserializeObject(result.Item1) as JArray;
            return arr?.Select(n => (int) n).ToArray();
        }

        public async Task<ProductPostProcessorData> GetProductById(string reindexUrl, int id)
        {
            var url = Url.Combine(reindexUrl, id.ToString());
            var result = await GetContent(url);
            var obj = JsonConvert.DeserializeObject(result.Item1) as JObject;
            if (obj != null)
            {
                var product = (JObject)obj["product"];
                var regionTags = obj["regionTags"].ToObject<List<RegionTag>>().ToArray();
                return new ProductPostProcessorData(product, regionTags, result.Item2);
            }
            return null;
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
            _manager.Dispose();
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