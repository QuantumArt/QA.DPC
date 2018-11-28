﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Helpers;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public class ElasticSearchRepository : ISearchRepository
    {
        private readonly string _sourceQuery = "hits.hits.[?(@._source)]._source";
        private readonly int _timeout = 15;
        private readonly ILogger _logger;

        private Regex GetRegionTagRegex(string tag)
        {
            return new Regex($@"[<\[]replacement[>\]]tag={tag}[<\[]/replacement[>\]]");
        }

        public ElasticSearchRepository(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger(this.GetType());
        }

        public async Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options, DateTimeOffset defaultValue)
        {
            var result = await GetContent(GetJsonQuery(productIds, true), options);
            var dates = JObject.Parse(result).SelectTokens($"{_sourceQuery}.UpdateDate").Select(n => (DateTimeOffset)n).ToArray();
            return dates.Any() ? dates.Max() : defaultValue;
        }

        private string GetJsonQuery(int[] productIds, bool onlyModified = false)
        {
            var ids = string.Join(", ", productIds.Select(n => $@"""{n.ToString()}""").ToArray());
            var fieldsFilter = (onlyModified) ? @"""_source"": [""UpdateDate""]," : "";
            var query = $@"{{ {fieldsFilter} ""size"" : 500, ""query"" : {{ ""ids"" : {{ ""values"" : [{ids}] }}}}}}";
            return query;
        }

        private string GetRegionFromRoamingQuery(string regionAlias)
        {
            return $@"{{ ""_source"": [""Region.Id""], ""query"" : {{ ""term"" : {{ ""Region.Alias"" : ""{regionAlias}"" }}}}}}";
        }

        private string GetRegionQuery(string regionAlias)
        {
            return $@"{{ ""query"" : {{ ""term"" : {{ ""Alias"" : ""{regionAlias}"" }}}}}}";
        }
        
        private string GetDefaultRegionForMnrQuery()
        {
            return $@"{{ ""_source"": [""Alias""], ""query"" : {{ ""term"" : {{ ""IsDefaultForMnr"": ""true"" }}}}}}";
        }


        private string GetMrQuery(string[] regionAliases)
        {
            var regions = string.Join(", ", regionAliases.Select(n => $@"""{n.ToString()}""").ToArray());
            return $@"{{ ""_source"": [""Region.Parent.Alias""], ""query"" : {{ ""terms"" : {{ ""Region.Alias"" : [{regions}] }}}}}}";
        }

        private string GetRoamingCountryQuery(string code)
        {
            return 
                $@"{{ 
                    ""_source"": [""*""], 
                    ""query"" : {{ 
                        ""bool"" : {{ 
                           ""should"" : [
                                {{ ""term"" : 
                                    {{ ""Country.Code"" : ""{code}"" }}
                                }}, 
                                {{ ""term"" : 
                                    {{ ""Alias"" : ""{code}"" }}
                                }}
                            ] 
                        }} 
                    }}
                }}";
        }

        private string GetRoamingScaleQuery(string code, bool isB2C)
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
                    ""query"": {{
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
                        }}
                    }}
                }}
            }}";
        }


        public async Task<JObject[]> GetProducts(int[] productIds, SearchOptions options)
        {
            var result = await GetContent(GetJsonQuery(productIds), options);

            result = await ProcessRegionTags(productIds, result, options);

            var hits = JObject.Parse(result).SelectTokens(_sourceQuery).ToArray();

            return hits.Select(n => (JObject)n).ToArray();
        }

        public async Task<bool> IsOneMacroRegion(string[] regions, SearchOptions options)
        {
            var newOptions = options.Clone();
            newOptions.TypeName = "RoamingRegion";
            var regionResult = await GetContent(GetMrQuery(regions), newOptions);
            var aliases = JObject.Parse(regionResult)
                .SelectTokens($"{_sourceQuery}.Region.Parent.Alias").Select(n => n.ToString()).ToArray();
            return aliases.Length > 1 && aliases.Distinct().Count() == 1;
        }

        public async Task<int[]> GetRoamingScaleForCountry(string code, bool isB2C, SearchOptions options)
        {
            var query = GetRoamingScaleQuery(code, isB2C);
            var country = await GetContent(query, options);
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
            var query = GetRoamingCountryQuery(code.ToLowerInvariant());
            var regionResult = await GetContent(query, options);
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
            var tagResult = await GetContent(GetJsonQuery(regionTagIds), options);
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
            var regionResult = await GetContent(GetRegionQuery(newOptions.HomeRegion), newOptions);
            var homeRegionIdToken = JObject.Parse(regionResult)
                .SelectTokens(_sourceQuery)?.FirstOrDefault();
            return (JObject)homeRegionIdToken;
        }

        public async Task<string> GetDefaultRegionAliasForMnr(SearchOptions options)
        {
            var newOptions = options.Clone();
            newOptions.TypeName = "Region";
            var regionResult = await GetContent(GetDefaultRegionForMnrQuery(), newOptions);
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

        private IElasticClient GetElasticClient(SearchOptions options)
        {
            return ElasticConfiguration.GetElasticClient(options.IndexName, options.BaseAddress, _logger, false, _timeout);
        }


        public async Task<string> GetContent(string json, SearchOptions options)
        {
            var client = GetElasticClient(options);

            ElasticsearchResponse<string> response; 
            if (options.TypeName == null)
            {
                response = await client.LowLevel.SearchAsync<string>(client.ConnectionSettings.DefaultIndex, json);
            }
            else
            {
                response = await client.LowLevel.SearchAsync<string>(client.ConnectionSettings.DefaultIndex, options.TypeName, json);
            }

            if (response.Success)
            {
                return response.Body;
            }

            return string.Empty;
        }
    }
}