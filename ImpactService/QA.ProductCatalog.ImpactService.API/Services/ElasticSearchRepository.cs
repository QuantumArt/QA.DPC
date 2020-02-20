using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using Polly.Extensions.Http;
using Polly.Registry;
using QA.ProductCatalog.ImpactService.API.Helpers;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class ElasticSearchRepository : ISearchRepository
    {
        private readonly string _sourceQuery = "hits.hits.[?(@._source)]._source";
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly PolicyRegistry _policyRegistry;
        private readonly ConfigurationOptions _options;
        

        private Regex GetRegionTagRegex(string tag)
        {
            return new Regex($@"[<\[]replacement[>\]]tag={tag}[<\[]/replacement[>\]]");
        }

        public ElasticSearchRepository(
            IHttpClientFactory clientFactory, 
            PolicyRegistry policyRegistry, 
            IOptions<ConfigurationOptions> elasticIndexOptionsAccessor
         )
        {
            _logger = LogManager.GetCurrentClassLogger();
            _clientFactory = clientFactory;
            _policyRegistry = policyRegistry;
            _options = elasticIndexOptionsAccessor.Value;
        }

        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options, DateTimeOffset defaultValue)
        {
            var query = GetJsonQuery(productIds, options.QueryType, true);
            var result = await GetContent(query, options,"Receiving product last updated date");
            var dates = JObject.Parse(result).SelectTokens($"{_sourceQuery}.UpdateDate").Select(n => (DateTimeOffset)n).ToArray();
            return dates.Any() ? dates.Max() : defaultValue;
        }

        private string GetJsonQuery(int[] productIds, string type, bool onlyModified = false)
        {
            var ids = string.Join(", ", productIds.Select(n => $@"""{n.ToString()}""").ToArray());
            var fieldsFilter = (onlyModified) ? @"""_source"": [""UpdateDate""]," : "";
            var query = $@"{{ {fieldsFilter} ""size"" : 500, ""query"" : {{ {GetTypeQuery(type, $@" ""ids"" : {{ ""values"" : [{ids}] }} ")} }} }}";
            return query;
        }

        private string GetRegionFromRoamingQuery(string regionAlias, string type)
        {
            return $@"{{ ""_source"": [""Region.Id""], ""query"" : {{ {GetTypeQuery(type, $@" ""term"" : {{ ""Region.Alias"" : ""{regionAlias}"" }} ")} }} }}";
        }

        private string GetRegionQuery(string regionAlias, string type)
        {
            return $@"{{ ""query"" : {{ {GetTypeQuery(type, $@" ""term"" : {{ ""Alias"" : ""{regionAlias}"" }} ")} }} }}";
        }
        
        private string GetDefaultRegionForMnrQuery(string type)
        {
            return $@"{{ ""_source"": [""Alias""], ""query"" : {{ {GetTypeQuery(type, $@" ""term"" : {{ ""IsDefaultForMnr"": ""true"" }} ")} }} }}";
        }


        private string GetMrQuery(string[] regionAliases, string type)
        {
            var regions = string.Join(", ", regionAliases.Select(n => $@"""{n.ToString()}""").ToArray());
            return $@"{{ ""_source"": [""Region.Parent.Alias""], ""query"" : {{ {GetTypeQuery(type, $@" ""terms"" : {{ ""Region.Alias"" : [{regions}] }} ")} }} }}";
        }

        private string GetRoamingCountryQuery(string code, string type)
        {
            return
                $@"{{ 
                    ""_source"": [""*""], 
                    ""query"" : {{ {GetTypeQuery(type, $@"
                        ""bool"" : {{ 
                           ""should"" : [
                                {{ ""term"" : 
                                    {{ ""Country.Code"" : ""{code}"" }}
                                }}, 
                                {{ ""term"" : 
                                    {{ ""Alias"" : ""{code}"" }}
                                }}
                            ] 
                        }}")}
                    }}
                }}";
        }

        private string GetRoamingScaleQuery(string code, bool isB2C, string type)
        {
            var modifier = isB2C ? "IsForMainSite" : "IsForCorpSite";
            return

                $@"{{ 
                    ""from"": 0,
                    ""size"": 1,
                    ""_source"": {{
                        ""include"": [
                            ""ServicesOnRoamingScale.Service.MarketingProduct.Title"",
                            ""ServicesOnRoamingScale.Service.Regions.Alias"",
                            ""ServicesOnRoamingScale.Service.Id"",
                            ""Id""
                        ]
                    }},
                    ""query"": {{ {GetTypeQuery(type, $@"
                        ""bool"": {{
                            ""should"": [ 
                                {{
                                    ""bool"": {{
                                        ""must"": [ 
                                            {{
                                                ""match_phrase"": {{
                                                    ""MarketingProduct.Countries.Country.Code"": {{
                                                        ""query"": ""{code}""
                                                    }}
                                                }}
                                            }},
                                            {{
                                                ""term"": {{ ""MarketingProduct.Modifiers.Alias"": ""{modifier}"" }}
                                            }}
                                        ]
                                    }}
                                }},
                                {{
                                    ""bool"": {{
                                        ""must"": [
                                            {{
                                                ""term"": {{ ""MarketingProduct.Countries.Alias"": ""{code}"" }}
                                            }},
                                            {{
                                                ""term"": {{ ""MarketingProduct.Modifiers.Alias"": ""{modifier}"" }}
                                            }}
                                        ]
                                    }}
                                }}
                            ]
                        }}")}
                    }}
            }}";
        }

        private string GetTypeQuery(string type, string subquery)
        {
            if (type == null)
                return subquery;
            else
                return
                    $@"""bool"" : {{
                        ""must"" : [
                            {{ ""term"" : {{ ""Type"" : ""{type}"" }} }},
                            {{ {subquery} }}
                        ]
                    }}";
        }


        public async Task<JObject[]> GetProducts(int[] productIds, SearchOptions options)
        {
            var query = GetJsonQuery(productIds, options.QueryType);
            var result = await GetContent(query,  options, "Receiving products");

            result = await ProcessRegionTags(productIds, result, options);

            var hits = JObject.Parse(result).SelectTokens(_sourceQuery).ToArray();

            return hits.Select(n => (JObject)n).ToArray();
        }

        public async Task<bool> IsOneMacroRegion(string[] regions, SearchOptions options)
        {
            var newOptions = options.Clone();
            newOptions.TypeName = "RoamingRegion";
            var query = GetMrQuery(regions, options.QueryType);
            var regionResult = await GetContent(query, newOptions, "Check for one macroregion");
            var aliases = JObject.Parse(regionResult)
                .SelectTokens($"{_sourceQuery}.Region.Parent.Alias").Select(n => n.ToString()).ToArray();
            return aliases.Length > 1 && aliases.Distinct().Count() == 1;
        }

        public async Task<int[]> GetRoamingScaleForCountry(string code, bool isB2C, SearchOptions options)
        {
            var query = GetRoamingScaleQuery(code, isB2C, options.QueryType);
            var country = await GetContent(query, options, "Receiving roaming scale for country");
            var countryObj = JObject.Parse(country).SelectToken(_sourceQuery);
            if (countryObj == null)
                throw new Exception($"Roaming scale for country with code '{code}' not found");
            var result = new List<int> {int.Parse(countryObj["Id"].ToString())};
            var services = countryObj.SelectTokens($"ServicesOnRoamingScale.[?(@.Service)].Service").ToArray();
            var servicesWithAliases = services
                .Select(service => new
                {
                    service, aliases = new HashSet<string>(service.SelectTokens("Regions.[?(@.Alias)].Alias").Select(n => n.ToString()))
                }).ToArray();
            
            result.AddRange(servicesWithAliases
                .Where(n => n.aliases.Contains(options.HomeRegion))
                .Select(n => (int) n.service.SelectToken("Id")));
            return result.ToArray();
        }

        public async Task<JObject> GetRoamingCountry(string code, SearchOptions options)
        {
            var query = GetRoamingCountryQuery(code.ToLowerInvariant(), options.QueryType);
            var regionResult = await GetContent(query, options, "Receiving roaming country");
            var obj = JObject.Parse(regionResult);
            var result = (JObject)obj.SelectToken(_sourceQuery);
            if (result == null)
                throw new Exception($"Roaming country code '{code}' not found");
            return result;
        }

        private async Task<string> ProcessRegionTags(int[] productIds, string input, SearchOptions options)
        {
            var result = input;
            var tagsToProcess = await GetTagsToProcess(productIds, options);
            for (var i = 1; i <= 2; i++)
            {
                foreach (var tag in tagsToProcess)
                {
                    result = GetRegionTagRegex(tag.Key).Replace(result, tag.Value);
                }
            }
            return result;
        }

        private async Task<Dictionary<string, string>> GetTagsToProcess(int[] productIds, SearchOptions options)
        {
            var regionTagIds = productIds.Select(n => 0 - n).ToArray();
            var query = GetJsonQuery(regionTagIds, options.QueryType);
            var tagResult = await GetContent(query, options, "Receiving regional tags to process");
            var documents = JObject.Parse(tagResult).SelectTokens(_sourceQuery).ToArray();
            var tags = documents
                    .SelectMany(n => (JArray)n.SelectToken("RegionTags"))
                    .ToArray();

            var homeRegionId = GetHomeRegionId(options.HomeRegionData);


            var tagsToProcess = new Dictionary<string, string>();

            foreach (var tag in tags)
            {
                var title = tag["Title"].ToString();

                if (tagsToProcess.ContainsKey(title)) continue;

                var value = GetTagValue(homeRegionId, tag);

                tagsToProcess.Add(title, value.Substring(1, value.Length - 2));
            }

            return tagsToProcess;
        }

        private int GetHomeRegionId(JObject homeRegion)
        {
            return (homeRegion != null) ? (int) homeRegion.SelectToken("Id") : 0;
        }

        public async Task<JObject> GetHomeRegion(SearchOptions options)
        {
            if (options.HomeRegion == null) return null;

            var newOptions = options.Clone();
            newOptions.TypeName = "Region";

            var query = GetRegionQuery(newOptions.HomeRegion, options.QueryType);
            var regionResult = await GetContent(query, newOptions, "Receiving home region");
            var homeRegionIdToken = JObject.Parse(regionResult)
                .SelectTokens(_sourceQuery)?.FirstOrDefault();
            return (JObject)homeRegionIdToken;
        }

        public async Task<string> GetDefaultRegionAliasForMnr(SearchOptions options)
        {
            var newOptions = options.Clone();
            newOptions.TypeName = "Region";
            var query = GetDefaultRegionForMnrQuery(options.QueryType);
            var regionResult = await GetContent(query, newOptions, "Receiving default region alias");
            return JObject.Parse(regionResult)
                .SelectTokens(_sourceQuery)?.FirstOrDefault()?.SelectToken("Alias")?.ToString();
        }

        private static string GetTagValue(int homeRegionId, JToken tag)
        {
            string value = null;
            if (homeRegionId != 0)
            {
                var values = tag["Values"].SelectTokens("[?(@.Value)]");
                value = values.FirstOrDefault(n => ((JArray)n["RegionsId"]).Select(m => (int)m).Contains(homeRegionId))
                   ?.SelectToken("Value")?.ToString();
                     
            }
            value = JsonConvert.ToString(value ?? (tag["Values"].Any() ? tag["Values"][0]["Value"].ToString() : ""));
            return value;
        }

        private ElasticClient GetElasticClient(SearchOptions options)
        {
            return new ElasticClient(_clientFactory, _policyRegistry, options.IndexName, options.BaseUrls, _options);
        }


        public async Task<string> GetContent(string json, SearchOptions options, string message = null)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace().Message(message ?? "Query to Elastic search")
                    .Property("json", json)
                    .Property("searchOptions", options)
                    .Write();
            }
            
            return await GetElasticClient(options).SearchAsync(options.UrlType, json);
        }

        public async Task<bool> GetIndexIsTyped(SearchOptions options)
        {
            if (_logger.IsTraceEnabled)
            {
                _logger.Trace("Check for typed index");
            }

            var client = GetElasticClient(options);
            var info = await client.SearchAsync(null, null);
            var version = JObject.Parse(info).SelectToken("version.number").Value<string>();
            return version[0] <= '5';
        }
    }
}
