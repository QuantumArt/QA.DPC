using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Registry;

namespace QA.ProductCatalog.ImpactService.API.Services
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
        private readonly string _elasticBasicToken;
        
        public ElasticClient(IHttpClientFactory factory, PolicyRegistry registry, string indexName, string[] uris, ConfigurationOptions options)
        {
            _factory = factory;
            _registry = registry;
            _indexName = indexName;
            _uris = uris;
            _logger = LogManager.GetCurrentClassLogger();
            _timeout = TimeSpan.FromSeconds(options.HttpTimeout);
            _faluiresAccepted = options.FailuresBeforeCircuitBreaking;
            _circuitBreakingInterval = TimeSpan.FromSeconds(options.CircuitBreakingInterval);
            _exceptions = new List<Exception>();
            _elasticBasicToken = options.ElasticBasicToken;
        }

        public async Task<string> SearchAsync(string type, string json)
        {
            var randomIndexes = GetRandomIndexes();
           _exceptions.Clear();
            
            foreach (var index in randomIndexes)
            {
                var baseUri = _uris[index];
                
                var response = await GetHttpResponse(baseUri, type, json);

                if (response == null) continue;

                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return result;

                HandleErrorResult(response.StatusCode, result);
            }

            throw GetElasticClientException(json);
        }


        private Exception GetElasticClientException(string json)
        {
            var resultEx = (_exceptions.Count > 1)
                ? new AggregateException("No successful results", _exceptions)
                : _exceptions.FirstOrDefault() ?? new ApplicationException("Normally shouldn't be thrown");

            throw new ElasticClientException("Elastic call failed: " + resultEx.Message, resultEx) {Request = json};
        }

        private void HandleErrorResult(HttpStatusCode code, string result)
        {
            var message = $"HTTP Error. Code: {code}. Response: {result}";
            _exceptions.Add(new HttpRequestException(message));
            _logger.Info(message);
        }
   
        private async Task<HttpResponseMessage> GetHttpResponse(string baseUri, string type, string json)
        {
            HttpResponseMessage response = null;
            var indexUri = $"{baseUri}/{_indexName}";
            var policy = GetOrCreatePolicy(indexUri);
            var client = CreateClient(baseUri);
            var uri = GetUri(type);
            try
            {
                if (type == null && json == null)
                {
                    response = await GetAsync(client, policy, null);
                }
                else
                {
                    response = await PostAsync(client, policy, uri, json);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Info(ex, "Elastic connection error for {indexUri}", indexUri);
                _exceptions.Add(ex);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.Info(ex, "Circuit broken for {indexUri}", indexUri);
                _exceptions.Add(ex);
            }
            catch (OperationCanceledException ex)
            {
                _logger.Info(ex, "Elastic connection timeout for {indexUri}", indexUri);
                _exceptions.Add(ex);
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Unexpected exception for {indexUri}", indexUri);
                _exceptions.Add(ex);
            }

            return response;
        }

        private static Task<HttpResponseMessage> PostAsync(HttpClient client, IAsyncPolicy<HttpResponseMessage> policy,
            string uri, string json)
        {            
            return policy.ExecuteAsync(async () => await client.PostAsync(uri, new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
            ));
        }

        private static Task<HttpResponseMessage> GetAsync(HttpClient client, IAsyncPolicy<HttpResponseMessage> policy, string uri)
        {
            return policy.ExecuteAsync(async () => await client.GetAsync(uri));
        }

        private int[] GetRandomIndexes()
        {
            var rnd = new Random();
            var randomIndexes = Enumerable.Range(0, _uris.Length).OrderBy(x => rnd.Next()).ToArray();
            return randomIndexes;
        }

        private string GetUri(string type)
        {
            return (type != null) ? $"{_indexName}/{type}/_search" : $"{_indexName}/_search";
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
            if (!string.IsNullOrWhiteSpace(_elasticBasicToken))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {_elasticBasicToken}");
            }
            return client;
        }
    }
    
    public class ElasticClientException : Exception
    {

        public string Request { get; set; }

        public ElasticClientException()
        {
        }

        public ElasticClientException(string message) : base(message)
        {
        }

        public ElasticClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
    
    


