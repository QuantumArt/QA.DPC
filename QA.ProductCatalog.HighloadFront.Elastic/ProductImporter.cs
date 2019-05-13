using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
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
        private HttpClient _client;

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
            string customerCode)

        {
            _logger = loggerFactory.CreateLogger(GetType());
            _manager = manager;
            _configuration = configuration;
            _options = options;
            _dataOptions = dataOptions;
            _customerCode = customerCode;
            _client = httpClientFactory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Clear();            
        }

        public bool ValidateInstance(string language, string state)
        {
            var reindexUrl = _configuration.GetReindexUrl(language, state);
            _logger.LogInformation($"Checking instance ({language}, {state}) ...");            
            var url = $"{reindexUrl}/ValidateInstance";
            var result = GetContent(url).Result;
            var validation = JsonConvert.DeserializeObject<bool>(result.Item1);
            _logger.LogInformation($"Validation result for instance ({language}, {state}): {validation}");            
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
                    executionContext.Message = message;
                    throw;
                }
                _logger.LogInformation($"Products from chunk {index} received. Starting bulk import...");

                var result = await _manager.BulkCreateAsync(data, language, state);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Bulk import for chunk {index} succeeded.");
                    index++;
                }
                else
                {
                    string message = $"Cannot proceed bulk import for chunk {index}: {result}";
                    _logger.LogInformation(message);
                    executionContext.Message = message;
                    throw result.GetException();
                }

                progress += (float)100 / chunks.Length;

                executionContext.SetProgress((byte)progress);
            }

            executionContext.Message = "Import completed";
        }

        public async Task<Tuple<string, DateTime>> GetContent(string url)
        {
            url += $"?customerCode={_customerCode}&instanceId={_dataOptions.InstanceId}";
            _logger.LogDebug($"Requesting URL: {url}");
            var response = await _client.GetAsync(url);
            var modified = response.Content.Headers.LastModified?.DateTime ?? DateTime.Now;
            var result = (!response.IsSuccessStatusCode) ? "" : await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Received {response.StatusCode} for URL: {url}");
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