using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.HighloadFront.Elastic.Extensions;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ProductImporter
    {
        private readonly ILogger _logger;

        private readonly ElasticConfiguration _configuration;

        private readonly ProductManager _manager;

        private readonly HarvesterOptions _options;
        private readonly DataOptions _dataOptions;

        private readonly string _customerCode;

        public ProductImporter(HarvesterOptions options, DataOptions dataOptions, ElasticConfiguration configuration, ProductManager manager, ILoggerFactory loggerFactory, string customerCode)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _manager = manager;
            _configuration = configuration;
            _options = options;
            _dataOptions = dataOptions;
            _customerCode = customerCode;
        }

        public bool ValidateInstance(string language, string state)
        {
            var reindexUrl = _configuration.GetReindexUrl(language, state);
            var url = $"{reindexUrl}/ValidateInstance";
            var result = GetContent(url).Result;
            var validation = JsonConvert.DeserializeObject<bool>(result.Item1);
            return validation;
        }

        public async Task ImportAsync(ITaskExecutionContext executionContext, string language, string state)
        {
            if(executionContext.IsCancellationRequested)
            {
                executionContext.IsCancelled = true;

                return;
            }
            
            var url = _configuration.GetReindexUrl(language, state);
            _logger.LogInformation("Начинаем импорт данных. Запрашиваем список продуктов...");
            var ids = await GetIds(url);
            _logger.LogInformation($"Получен список продуктов. Количество: {ids.Length}");
            _logger.LogInformation($"Бьем продукты на порции по {_options.ChunkSize}...");
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
           

                _logger.LogInformation($"Запрашиваем порцию №{index}...");
                var dataTasks = chunk.Select(n => GetProductById(url, n));

                ProductPostProcessorData[] data;
                try
                {
                    data = await Task.WhenAll(dataTasks);
                }
                catch (Exception)
                {
                    string message = "Не удалось получить продукты от удаленного сервиса.";
                    _logger.LogInformation(message);
                    executionContext.Message = message;
                    throw;
                }
                _logger.LogInformation("Продукты получены.");


                _logger.LogInformation("Начинаем запись...");

                var result = await _manager.BulkCreateAsync(data, language, state);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Запись прошла успешно.");
                    index++;
                }
                else
                {
                    string message = $"Не удалось импортировать все продукты. {result}";
                    _logger.LogInformation(message);
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
                url += $"?customerCode={_customerCode}&instanceId={_dataOptions.InstanceId}";
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
            var relUri = $"{reindexUrl}/{id}";
            var result = await GetContent(relUri);
            var obj = JsonConvert.DeserializeObject(result.Item1) as JObject;
            if (obj == null)
            {
                _logger.LogError($"Cannot parse JSON from url {relUri}");
                return null;
            }
            var product = (JObject)obj["product"];
            var regionTags = obj["regionTags"]?.ToObject<List<RegionTag>>()?.ToArray() ?? new RegionTag[] {};
            return new ProductPostProcessorData(product, regionTags, result.Item2);
        }

    }
}