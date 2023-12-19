using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ProductImporter
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger _logger;

        private readonly ElasticConfiguration _configuration;

        private readonly ProductManager _manager;

        private readonly HarvesterOptions _options;
        private readonly DataOptions _dataOptions;

        private readonly string _customerCode;

        public ProductImporter(
            HarvesterOptions options, 
            DataOptions dataOptions, 
            ElasticConfiguration configuration, 
            ProductManager manager, 
            ILoggerFactory loggerFactory, 
            IHttpClientFactory httpClientFactory,
            IIdentityProvider identityProvider)

        {
            _logger = loggerFactory.CreateLogger(GetType());
            _manager = manager;
            _configuration = configuration;
            _options = options;
            _dataOptions = dataOptions;
            _customerCode = identityProvider.Identity.CustomerCode;
            _factory = httpClientFactory;
        }

        public bool ValidateInstance(string language, string state)
        {
            var reindexUrl = _configuration.GetReindexUrl(language, state);
            _logger.LogInformation($"Checking instance ({language}, {state}) ...");
            var url = $"{reindexUrl}/ValidateInstance";
            var result = GetContent(url).Result;
            var validation = JsonNode.Parse(result.Item1).Deserialize<bool>();
            _logger.LogInformation($"Validation result for instance ({language}, {state}): {validation}");
            return validation;
        }

        public async Task ImportAsync(ITaskExecutionContext executionContext, string language, string state, Dictionary<string, IProductStore> stores, string newIndex)
        {
            if(executionContext.IsCancellationRequested)
            {
                executionContext.IsCancelled = true;

                return;
            }
            
            var url = _configuration.GetReindexUrl(language, state);
            _logger.LogInformation("Starting import...");
            var ids = await GetIds(url);
            _logger.LogInformation($"Product list received. Length: {ids.Length}. Splitting products into chunks by {_options.ChunkSize}...");
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


                var enumerable = chunk as int[] ?? chunk.ToArray();
                _logger.LogInformation($"Chunk {index} with ids ({string.Join(",", enumerable)}) requested...");
                var dataTasks = enumerable.Select(n => GetProductById(url, n));

                ProductPostProcessorData[] data;
                try
                {
                    data = await Task.WhenAll(dataTasks);
                }
                catch (Exception ex)
                {
                    string message = $"An error occurs while receiving products for chunk {index}";
                    _logger.LogError(ex, message);
                    executionContext.Result = ActionTaskResult.Error(message);
                    throw;
                }
                _logger.LogInformation($"Products from chunk {index} received. Starting bulk import...");

                var result = await _manager.BulkCreateAsync(data, language, state, stores, newIndex);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Bulk import for chunk {index} succeeded.");
                    index++;
                }
                else
                {
                    string message = $"Cannot proceed bulk import for chunk {index}: {result}";
                    _logger.LogError(message);
                    executionContext.Result = ActionTaskResult.Error(message);
                    throw result.GetException();
                }

                progress += (float)100 / chunks.Length;

                executionContext.SetProgress((byte)progress);
            }

            executionContext.Result = ActionTaskResult.Success("Import completed");
        }

        private async Task<Tuple<string, DateTime>> GetContent(string url)
        {
            url += $"?customerCode={_customerCode}&instanceId={_dataOptions.InstanceId}";
            _logger.LogDebug($"Requesting URL: {url}");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.Timeout = TimeSpan.FromMinutes(5);
            var response = await client.GetAsync(url);
            var modified = response.Content.Headers.LastModified?.DateTime ?? DateTime.Now;
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"URL {url} failed with code {response.StatusCode}");
                return new Tuple<string, DateTime>("", modified);
            }
            var result = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Received {response.StatusCode} for URL: {url}");
            return new Tuple<string, DateTime>(result, modified);
        }

        private async Task<int[]> GetIds(string reindexUrl)
        {
            var result = await GetContent(reindexUrl);
            var arr = JsonNode.Parse(result.Item1);
            return arr.Deserialize<int[]>();
        }

        private async Task<ProductPostProcessorData> GetProductById(string reindexUrl, int id)
        {
            var relUri = $"{reindexUrl}/{id}";
            Tuple<string, DateTime> result;
            try
            {
                result = await GetContent(relUri);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError($"Cannot get product {id} with url {relUri}");
                return null;
            }

            using var doc = JsonDocument.Parse(result.Item1);
            JsonElement obj = !string.IsNullOrEmpty(result.Item1) ? doc.RootElement.Clone() : new JsonElement();
            if (obj.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogError($"Cannot parse JSON for product {id} with url {relUri}");
                return null;
            }
            var product = obj.GetProperty("product");
            return new ProductPostProcessorData(product, _manager.GetRegionTags(obj), result.Item2);
        }

    }
}