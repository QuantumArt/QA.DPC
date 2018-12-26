using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Registry;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{

    public class ElasticClient
    {
        private readonly IHttpClientFactory _factory;

        private readonly TimeSpan _timeout;
        private readonly int _faluiresAccepted;
        private readonly TimeSpan _circuitBreakingInterval;
        private readonly string[] _uris;
        private readonly string _indexName;
        private readonly ILogger _logger;
        private readonly PolicyRegistry _registry;
        private readonly List<Exception> _exceptions;
        
        public ElasticClient(IHttpClientFactory factory, PolicyRegistry registry, string indexName, string[] uris,
            ILogger logger, DataOptions options)
        {
            _factory = factory;
            _registry = registry;
            _indexName = indexName;
            _uris = uris;
            _logger = logger;
            _timeout = TimeSpan.FromSeconds(options.ElasticTimeout);
            _faluiresAccepted = options.FailuresBeforeCircuitBreaking;
            _circuitBreakingInterval = TimeSpan.FromSeconds(options.CircuitBreakingInterval);
            _exceptions = new List<Exception>();
        }

        public async Task<string> SearchAsync(string type, string json)
        {
            var searchParams = CreateElasticRequestParams(HttpMethod.Post, "_search", type);
            return await QueryAsync(searchParams, json);
        }
        
        public async Task<string> BulkAsync(string commands)
        {
            var searchParams = CreateElasticRequestParams(HttpMethod.Post, "_bulk");
            return await QueryAsync(searchParams, commands);
        }

        public async Task<bool> IndexExistsAsync()
        {
            var searchParams = CreateElasticRequestParams(HttpMethod.Head);
            try
            {
                await QueryAsync(searchParams, null);
                return true;                
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> DeleteIndexAsync()
        {
            var eparams = CreateElasticRequestParams(HttpMethod.Delete);
            return await QueryAsync(eparams, null);           
        }
        
        public async Task<string> CreateIndexAsync(string json)
        {
            var eparams = CreateElasticRequestParams(HttpMethod.Put);
            return await QueryAsync(eparams, json);          
        }

        public async Task<bool> DocumentExistsAsync(string id, string type)
        {
            var searchParams = CreateElasticRequestParams(HttpMethod.Head, id, type);
            try
            {
                await QueryAsync(searchParams, null);
                return true;                
            }
            catch (Exception)
            {
                return false;
            }         
        }

        public async Task<string> FindSourceByIdAsync(string id, string[] filter)
        {
            var operation = $"{id}/_source";
            var eparams = CreateElasticRequestParams(HttpMethod.Get, operation, "_all");
            if (filter != null)
            {
                eparams.UrlParams.Add("_source_include", String.Join(",", filter));
            }
            
            return await QueryAsync(eparams, null);
        }

        public async Task<string> PutAsync(string id, string type, string json)
        {
            var eparams = CreateElasticRequestParams(HttpMethod.Put, id, type);
            return await QueryAsync(eparams, json);
        }

        public async Task<string> UpdateAsync(string id, string type, string json)
        {
            var eparams = CreateElasticRequestParams(HttpMethod.Post, $"{id}/_update", type);
            return await QueryAsync(eparams, json);
        }

        public async Task<string> DeleteAsync(string id, string type)
        {
            var eparams = CreateElasticRequestParams(HttpMethod.Delete, id, type);
            eparams.ThrowNotFound = false;
            return await QueryAsync(eparams, null);            
        }

        private ElasticRequestParams CreateElasticRequestParams(HttpMethod verb, string operation="", string type="")
        {
            return new ElasticRequestParams(verb, _indexName, operation, type);
        }

        private async Task<string> QueryAsync(ElasticRequestParams eparams, string json)
        {
            var randomIndexes = GetRandomIndexes();
           _exceptions.Clear();
            
            foreach (var index in randomIndexes)
            {
                var baseUri = _uris[index];
                
                var response = await GetHttpResponse(baseUri, eparams, json);

                if (response == null) continue;

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return result;

                var message = !string.IsNullOrEmpty(result) ? result : response.ReasonPhrase;
                
                HandleErrorResult(response.StatusCode, message);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    if (eparams.ThrowNotFound)
                    {
                        break;
                    }

                    return "Not Found";
                }
            }

            throw GetElasticClientException(eparams, json);
        }

        private Exception GetElasticClientException(ElasticRequestParams eparams, string json)
        {
            var resultEx = (_exceptions.Count > 1)
                ? new AggregateException("No successful results", _exceptions)
                : _exceptions.FirstOrDefault() ?? new ApplicationException("Normally shouldn't be thrown");

            throw new ElasticClientException("Elastic call failed: " + resultEx.Message, resultEx)
            {
                Request = json,
                BaseUrls = _uris,
                ElasticRequestParams = eparams
            };
        }

        private void HandleErrorResult(HttpStatusCode code, string result)
        {
            var message = $"HTTP Error. Code: {code}. Response: {result}";
            _exceptions.Add(new HttpRequestException(message));
            _logger.LogInformation(message);
        }

        private async Task<HttpResponseMessage> GetHttpResponse(string baseUri, ElasticRequestParams eparams, string json)
        {
            HttpResponseMessage response = null;
            var indexUri = $"{baseUri}/{_indexName}";
            var policy = GetOrCreatePolicy(indexUri);
            var client = CreateClient(baseUri);
            try
            {
                response = await SendAsync(client, eparams.Verb, policy, eparams.GetUri(), json);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Elastic connection error for {indexUri}", ex);
                _exceptions.Add(ex);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError($"Circuit broken for {indexUri}", ex);
                _exceptions.Add(ex);
            }

            return response;
        }

        private static Task<HttpResponseMessage> SendAsync(
            HttpClient client, 
            HttpMethod method, 
            IAsyncPolicy<HttpResponseMessage> policy,
            string uri, 
            string json
        )
        {
            HttpRequestMessage request = new HttpRequestMessage(method, uri);
            if (!String.IsNullOrEmpty(json))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return policy.ExecuteAsync(async () => await client.SendAsync(request));
        }

        private int[] GetRandomIndexes()
        {
            var rnd = new Random();
            var randomIndexes = Enumerable.Range(0, _uris.Length).OrderBy(x => rnd.Next()).ToArray();
            return randomIndexes;
        }

        private IAsyncPolicy<HttpResponseMessage> GetOrCreatePolicy(string indexUri)
        {
            if (!_registry.TryGet<IAsyncPolicy<HttpResponseMessage>>(indexUri, out var policy))
            {
                policy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(_faluiresAccepted, _circuitBreakingInterval);

                _registry[indexUri] = policy;
            }

            return policy;
        }

        private HttpClient CreateClient(string baseUri)
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.Timeout = _timeout;
            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
    
    


