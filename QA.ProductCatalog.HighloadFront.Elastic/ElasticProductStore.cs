using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.HighloadFront.PostProcessing;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore : IProductStore
    {
        private const string BaseSeparator = ",";
        private const string ProductIdField = "ProductId";

        private readonly IProductInfoProvider _productInfoProvider;

        protected ElasticConfiguration Configuration { get; }

        protected SonicElasticStoreOptions Options { get; }

        private ILogger Logger { get; }

        public ElasticProductStore(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory,
            IProductInfoProvider productInfoProvider)
        {
            Configuration = config;
            Options = options;
            _productInfoProvider = productInfoProvider;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public string GetId(JsonElement product)
        {
            return _productInfoProvider.GetId(product, Options.IdPath);
        }

        public virtual string GetType(JsonElement product)
        {
            return _productInfoProvider.GetType(product, Options.TypePath, Options.DefaultType);
        }
        
        public string GetId(JsonObject product)
        {
            return _productInfoProvider.GetId(product, Options.IdPath);
        }

        public virtual string GetType(JsonObject product)
        {
            return _productInfoProvider.GetType(product, Options.TypePath, Options.DefaultType);
        }

        protected virtual string BuildRowMetadata(string name, string type, string id)
        {
            return $"{{\"index\":{{\"_index\":\"{name}\",\"_type\":\"{type}\",\"_id\":\"{id}\"}}}}";
        }

        public async Task<SonicResult> BulkCreateAsync(IEnumerable<JsonElement> products, string language, string state, string newIndex)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));

            var failedResult = new List<SonicError>();
            var client = Configuration.GetElasticClient(language, state);

            var commands = products.Select(p =>
            {
                var id = GetId(p);
                if (id == null)
                {
                    failedResult.Add(SonicErrorDescriber.StoreFailure("Product has no id"));
                    return string.Empty;
                }

                var type = GetType(p);
                if (type == null)
                {
                    failedResult.Add(SonicErrorDescriber.StoreFailure($"Product {id} has no type"));
                    return string.Empty;
                }

                var metadata = BuildRowMetadata(newIndex, type, id);
                return $"{metadata}\n{p.ToString()}\n";
            });

            if (failedResult.Any()) return SonicResult.Failed(failedResult.ToArray());
            try
            {

                var responseText = await client.BulkAsync(string.Join(string.Empty, commands), newIndex);
                using var doc = JsonDocument.Parse(responseText);
                var responseJson = doc.RootElement.Clone();
                if (responseJson.TryGetProperty("errors", out JsonElement errValue) && errValue.GetBoolean())
                {
                    var errors = responseJson.GetProperty("items").AsNode().AsArray()
                        .Select(i => i["index"]?["error"])
                        .Where(i => i != null)
                        .Select(i => new
                        {
                            Code = i["type"].ToString(),
                            Description = i["reason"].ToString()
                        })
                        .Distinct()
                        .Select(i => new SonicError
                        {
                            Code = i.Code,
                            Description = i.Description,
                            Exception = new Exception($"{i.Code} : {i.Description}")
                        })
                        .ToArray();

                    return SonicResult.Failed(errors);
                }

                return SonicResult.Success;
            }
            catch (Exception ex)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(ex.Message, ex));
            }
        }

        public virtual async Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            return await client.FindSourceByIdAsync($"{options.Id}/_source", "_all", "_source_include", options?.PropertiesFilter?.ToArray());
        }

        public async Task<SonicResult> CreateAsync(JsonElement product, string language, string state)
        {
            var id = GetId(product);
            if (id == null)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure("Product has no id"));
            }

            var type = GetType(product);
            if (type == null)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure($"Product {id} has no type"));
            }

            var client = Configuration.GetElasticClient(language, state);

            try
            {
                await client.PutAsync(id, type, product.ToString());
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }

        public virtual async Task<string> GetIndicesByName(string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            try
            {
                return await client.GetIndicesByName();
            }
            catch (Exception ex)
            {
                throw new ElasticClientException("Unable to get indices by name.", ex);
            }
        }

        public virtual List<string> RetrieveIndexNamesFromIndicesResponse(string indices, string index)
        {
            try
            {
                using var doc = JsonDocument.Parse(indices);
                var indexData = doc.RootElement.AsNode().AsArray();
                var indexes = indexData.Select(p => p["index"].ToString()).ToList();
                return indexes.Where(x => x == index || x.StartsWith($"{index}.")).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to retrieve indexes from response.", ex);
            }
        }

        public virtual async Task<SonicResult> UpdateAsync(JsonElement product, string language, string state)
        {
            var id = GetId(product);
            if (id == null)
            {
                return SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure("Product has no id"));
            }

            var type = GetType(product);
            if (type == null)
            {
                return SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure($"Product {id} has no type"));
            }
            
            var client = Configuration.GetElasticClient(language, state);

            try
            {
                await client.UpdateAsync($"{id}/_update", type, product.ToString());
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }

        public async Task<SonicResult> DeleteAsync(JsonElement product, string language, string state)
        {

            var id = GetId(product);
            if (id == null)
            {
                return SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure("Product has no id"));
            }

            var client = Configuration.GetElasticClient(language, state); 
            try
            {
                var type = GetType(product);
                await client.DeleteAsync(id, type);
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }

        public async Task<bool> Exists(JsonElement product, string language, string state)
        {
            string id = GetId(product);
            string type = GetType(product);
            if (id == null || type == null)
            {
                return false;
            }
            var client = Configuration.GetElasticClient(language, state);
            return await client.DocumentExistsAsync(id, type);
        }

        public async Task ReplaceIndexesInAliasAsync(string language, string state, string newIndexName, string[] oldIndexes, string alias)
        {
            var client = Configuration.GetElasticClient(language, state);
            try
            {
                _ = await client.ReplaceIndexesInAliasAsync(GetReplaceIndexesRequest(newIndexName, oldIndexes, alias).ToString());
            }
            catch (Exception ex)
            {
                throw new ElasticClientException("Unable to add index to alias.", ex);
            }
        }

        public async Task DeleteIndexByNameAsync(string language, string state, string indexName)
        {
            var client = Configuration.GetElasticClient(language, state);
            try
            {
                await client.DeleteIndexByNameAsync(indexName);
            }
            catch (Exception ex)
            {
                throw new ElasticClientException("Unable to delete index.", ex);
            }
        }

        public async Task<string[]> GetIndexInAliasAsync(string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            var result = await client.GetAliasByNameAsync();

            if (result == ElasticClient.NotFoundResult)
            {
                return Array.Empty<string>();
            }

            using var doc = JsonDocument.Parse(result);
            var json = doc.RootElement.AsNode().AsObject();
            return json.Select(n => n.Key).ToArray();
        }

        protected virtual JsonObject GetReplaceIndexesRequest(string newIndex, string[] oldIndexes, string alias)
        {
            var actions = new JsonArray
            {
                GetAliasAction(newIndex, alias, "add")
            };

            foreach (string oldIndex in oldIndexes)
            {
                actions.Add(GetAliasAction(oldIndex, alias, "remove"));
            }

            return new JsonObject()
            {
                ["actions"] = actions
            };
        }

        protected virtual JsonObject GetAliasAction(string index, string alias, string action)
        {
            return new JsonObject()
            {
                [action] = new JsonObject()
                    {
                        ["index"] = index,
                        ["alias"] = alias
                    } 
            };
        }

        public async Task<string> CreateVersionedIndexAsync(string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            try
            {
                return await client.CreateVersionedIndexAsync(GetDefaultIndexSettings().ToString());
            }
            catch (Exception ex)
            {
                throw new ElasticClientException("Unable to create index.", ex);
            }
        }

        public virtual JsonObject GetDefaultIndexSettings()
        {
            var indexSettings = new JsonObject()
            {
                ["settings"] = new JsonObject()
                {
                    ["max_result_window"] = Options.MaxResultWindow,
                    ["mapping.total_fields.limit"] = Options.TotalFieldsLimit,
                    ["index"] = GetIndexAnalyzers()
                },
                ["mappings"] = GetMappings(Options.Types, Options.NotAnalyzedFields)
            };
            return indexSettings;
        }

        protected virtual JsonObject GetIndexAnalyzers()
        {
            return new JsonObject()
            {
                ["analysis"] = GetAnalysis()
            };
        }
        
        protected virtual JsonObject GetAnalysis()
        {
            return new JsonObject()
            {
                ["analyzer"] = GetAnalyzers(),
                ["tokenizer"] = GetTokenizers()
            };
        }

        protected virtual JsonObject GetTokenizers()
        {
            return new JsonObject()
            {
                ["edge_ngram_tokenizer"] = GetEdgeNgramTokenizer()
            };
        }

        protected virtual JsonObject GetEdgeNgramTokenizer()
        {
            return new JsonObject()
            {
                ["type"] = "edge_ngram",
                ["min_gram"] = Options.EdgeNgramOptions.MinNgram < 1 
                    ? throw new InvalidOperationException("Minimum length of ngram can't be less than 1.") 
                    : Options.EdgeNgramOptions.MinNgram,
                ["max_gram"] = Options.EdgeNgramOptions.MaxNgram < Options.EdgeNgramOptions.MinNgram
                    ? throw new InvalidOperationException("Maximum length of ngram can't be less then minimum length of ngram.")
                    : Options.EdgeNgramOptions.MaxNgram,
                ["token_chars"] = new JsonArray("letter", "digit")
            };
        }

        protected virtual JsonObject GetAnalyzers()
        {
            return new JsonObject ()
            {
                ["edge_ngram_analyzer"] = GetEdgeNgramAnalyzer(),
                ["edge_ngram_search_analyzer"] = GetEdgeNgramSearchAnalyzer()
            };
        }

        protected virtual JsonObject GetEdgeNgramAnalyzer()
        {
            return new JsonObject()
            {
                ["filter"] = new JsonArray("lowercase"),
                ["type"] = "custom",
                ["tokenizer"] = "edge_ngram_tokenizer"
            };
        }

        protected virtual JsonObject GetEdgeNgramSearchAnalyzer()
        {
            return new JsonObject()
            {
                ["tokenizer"] = "lowercase"
            };
        }

        public virtual async Task<string> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync(options.ActualType, q, cancellationToken);
        }

        protected JsonObject GetQuery(ProductsOptionsBase options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var q = new JsonObject()
            {
                ["from"] = options.ActualFrom,
                ["size"] = options.ActualSize,
                ["_source"] = new JsonArray(GetFields(options).Select(n => JsonValue.Create(n)).ToArray())
            };

            SetQuery(q, options);
            SetSorting(q, options);
            return q;
        }

        public async Task<string[]> GetTypesAsync(string language, string state)
        {
            var q = new JsonObject() 
            {
                ["aggs"] = new JsonObject()
                {
                    ["typesAgg"] = new JsonObject()
                    {
                        ["terms"] = new JsonObject()
                        {
                            ["field"] = "_type",
                            ["size"] = "200"
                        }
                    }
                },
                ["size"] = 0
            };
            var client = Configuration.GetElasticClient(language, state);
            var response = await client.SearchAsync(null, q.ToString());
            var result = JsonNode.Parse(response);
            return PostProcessHelper.Select(result, "aggregations.typesAgg.buckets.[?(@.key)].key")
                .Select(n => n.ToString())
                .ToArray();
        }   

        protected virtual JsonObject GetKeywordTemplate(string type, string field)
        {
            return new JsonObject()
            {
                [$"not_analyzed_{type}_{field}"] = new JsonObject()
                {
                   ["match_mapping_type"] = "string",
                   ["match"] = field,
                   ["mapping"] = new JsonObject()
                   {
                       ["type"] = "keyword"
                   }
                }
            };
        }

        protected JsonObject GetEdgeNgramTemplate(string type, string field)
        {
            return new JsonObject()
            {
                [$"edge_ngram_{type}_{field}"] = new JsonObject()
                {
                    ["match_mapping_type"] = "string",
                    ["match"] = field,
                    ["mapping"] = new JsonObject()
                    {
                        ["type"] = "text",
                        ["fields"] = new JsonObject()
                        {
                            ["keyword"] = new JsonObject()
                            {
                                ["type"] = "keyword",
                                ["ignore_above"] = 256
                            },
                            ["ngram"] = new JsonObject()
                            {
                                ["type"] = "text",
                                ["analyzer"] = "edge_ngram_analyzer",
                                ["search_analyzer"] = "edge_ngram_search_analyzer"
                            }
                        }
                    }
                }
            };
        }

        protected virtual JsonObject GetMapping(string type, string[] fields)
        {
            var templates = new JsonArray(fields.Select(n => GetKeywordTemplate(type, n)).ToArray());
            templates = AddEdgeNgramTemplates(templates, type);

            return new JsonObject()
            {
                ["dynamic_templates"] = templates
            };
        }

        protected virtual JsonArray AddEdgeNgramTemplates(JsonArray templates, string type)
        {
            if (Options.EdgeNgramOptions.NgramFields?.Length > 0)
            {
                foreach (string field in Options.EdgeNgramOptions.NgramFields)
                {
                    templates.Add(GetEdgeNgramTemplate(type, field));
                }
            }

            return templates;
        }

        public JsonObject GetMappings(string[] types, string[] fields)
        {
            var result = new JsonObject();
            foreach (var type in types)
            {
                result.Add(new (type, GetMapping(type, fields)));
            }
            return result;
        }

        protected virtual void SetQuery(JsonObject json, ProductsOptionsBase productsOptions)
        {
            KeyValuePair<string, JsonNode>? query = null;
            var filters = productsOptions.Filters;
            if (filters != null)
            {
                var conditions = filters.Select(n => CreateQueryElem(n, productsOptions.DisableOr, productsOptions.DisableNot, productsOptions.DisableLike));
                var shouldGroups = new List<List<KeyValuePair<string, JsonNode>?>>();
                var currentGroup = new List<KeyValuePair<string, JsonNode>?>();
                foreach (var condition in conditions)
                {
                    if (condition == null)
                        continue;

                    if (condition.Value.Value["or"] != null)
                    {
                        if (currentGroup.Any())
                        {
                            shouldGroups.Add(currentGroup);
                        }
                        condition.Value.Value["or"].Parent.AsObject().Remove("or");
                        currentGroup = new List<KeyValuePair<string, JsonNode>?>();
                    }

                    currentGroup.Add(condition.Value);
                }
                shouldGroups.Add(currentGroup);

                query = shouldGroups.Count == 1 ? Must(currentGroup) : Should(shouldGroups.Select(Must));
            }

            if (query != null)
            {
                json.Add(new KeyValuePair<string, JsonNode>("query", new JsonObject() { query.Value }));
            }
        }

        protected KeyValuePair<string, JsonNode>? CreateQueryElem(IElasticFilter filter, string[] disableOr, string[] disableNot, string[] disableLike)
        {
            var simpleFilter = filter as SimpleFilter;
            var rangeFilter = filter as RangeFilter;
            var queryFilter = filter as QueryFilter;
            var groupFilter = filter as GroupFilter;
            
            KeyValuePair<string, JsonNode>? result = null;

            if (groupFilter != null)
            {
                var props = groupFilter.Filters.Select(n =>
                    CreateQueryElem(n, disableOr, disableNot, disableLike)).ToArray();

                return (groupFilter.IsDisjunction) ? Should(props) : Must(props);
            }

            if (simpleFilter != null)
            {
                result = simpleFilter.Name != Options.TypePath
                    ? GetSingleFilterWithNot(simpleFilter.Name, simpleFilter.Values, simpleFilter.FromJson, disableOr, disableNot, disableLike)
                    : null;
            }

            if (rangeFilter != null)
            {
                result = new KeyValuePair<string, JsonNode>("range", 
                    new JsonObject() { GetSingleRangeQuery(rangeFilter) }
                );
            }

            if (queryFilter != null)
            {
                result = new KeyValuePair<string, JsonNode>("query_string",
                    new JsonObject()
                    {
                        ["query"] = queryFilter.Query,
                        ["lenient"] = true
                    });
            }

            if (filter.IsDisjunction && result != null)
            {
                result.Value.Value["or"] = true;
            }

            return result;
        }

        protected static KeyValuePair<string, JsonNode>? Must(IEnumerable<KeyValuePair<string, JsonNode>?> props)
        {
            return Bool(props, false);
        }

        protected static KeyValuePair<string, JsonNode>? Should(IEnumerable<KeyValuePair<string, JsonNode>?> props)
        {
            return Bool(props, true);
        }

        protected static KeyValuePair<string, JsonNode>? Bool(IEnumerable<KeyValuePair<string, JsonNode>?> props, bool isDisjunction)
        {
            if (props == null) return null;
            var arr = props.Where(n => n != null)
                .Select(n => new JsonObject(){n.Value})
                .ToArray();
            if (!arr.Any()) return null;

            return new KeyValuePair<string, JsonNode>("bool", new JsonObject()
            {
                [isDisjunction ? "should" : "must"] = arr.Length > 1 ? new JsonArray(arr) : arr[0]
            });
        }

        private KeyValuePair<string, JsonNode> GetSingleRangeQuery(RangeFilter elem)
        {
            var content = new JsonObject();
            if (elem.Floor != "")
            {
                content.Add("gte", elem.Floor);
            }
            if (elem.Ceiling != "")
            {
                content.Add("lte", elem.Ceiling);
            }
            return new KeyValuePair<string, JsonNode>(elem.Name, content);
        }

        private void SetSorting(JsonObject json, ProductsOptionsBase options)
        {
            if (!String.IsNullOrEmpty(options.Sort))
            {
                json.Add("sort", new JsonArray(
                    new JsonObject()
                    {
                        [options.Sort] = options.DirectOrder ? "asc" : "desc" 
                    }));
            }
        }

        private KeyValuePair<string, JsonNode>? GetSingleFilterWithNot(string field, StringValues values, bool fromJson, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            var conditions = StringValues.IsNullOrEmpty(values)
                ? null
                : values.Select(n => GetSingleFilterWithNot(field, n, disabledOrFields, disabledNotFields, disableLikeFields)).ToArray();

            if (conditions == null || conditions.Length == 0)
                return null;
            
            var op = (fromJson) ? "should" : "must";

            var result = conditions.Length == 1
                ? conditions.First()
                : new KeyValuePair<string, JsonNode>("bool", new JsonObject()
                {
                    [op] = new JsonArray(conditions.Select(n => (JsonNode) new JsonObject {n.Value}).ToArray())
                });

            return result;
        }

        private KeyValuePair<string, JsonNode>? GetSingleFilterWithNot(string field, string value, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            KeyValuePair<string, JsonNode> result;
            var actualSeparator = GetActualSeparator(field, disabledOrFields);
            var disableLike = disableLikeFields != null && disableLikeFields.Contains(field);
            var actualValue = GetActualValue(field, value, disabledNotFields, out var hasNegation);

            if (actualValue == "null")
            {
                result = Exists(field);
                hasNegation = !hasNegation;
            }
            else
            {
                result = GetSingleFilter(field, actualValue, actualSeparator, disableLike).Value;  
            }

            if (hasNegation)
            {
                result = new KeyValuePair<string, JsonNode>("bool", new JsonObject()
                {
                    ["must_not"] = new JsonObject { result }
                });
            }



            return result;
        }

        private string GetActualValue(string field, string value, string[] disabledNotFields, out bool hasNegation)
        {
            hasNegation = !string.IsNullOrEmpty(Options.NegationMark)
                          && (value.StartsWith(Options.NegationMark))
                          && (disabledNotFields == null || !disabledNotFields.Contains(field));

            return (hasNegation) ? value.Substring(Options.NegationMark.Length) : value;
        }

        private string GetActualSeparator(string field, string[] disabledOrFields)
        {
            var actualSeparator = !string.IsNullOrWhiteSpace(Options.ValueSeparator) ? Options.ValueSeparator : BaseSeparator;
            if (disabledOrFields != null)
            {
                actualSeparator = disabledOrFields.Contains(field) ? null : actualSeparator;
            }
            return actualSeparator;
        }

        protected KeyValuePair<string, JsonNode>? GetSingleFilter(string field, string value, string separator, bool  disableLike)
        {
            var isBaseField = field == Options.IdPath || field.EndsWith("." + Options.IdPath) || field == ProductIdField;
            var separators = (isBaseField) ? new[] {separator, BaseSeparator} : new[] {separator};
            var isSeparated = separators.Any(n => IsSeparated(value, n));             
            var values = (isSeparated) ? value.Split(separators, StringSplitOptions.None) : new string[0];

            if (isBaseField || IsNotAnalyzedField(field))
            {
                if (isSeparated)
                {
                    if (!disableLike && values.Any(IsWildcard))
                    {
                        return Wildcards(field, values);                        
                    }
                    else
                    {
                        return Terms(field, values);
                    }
                }
                else
                {
                    if (!disableLike && IsWildcard(value))
                    {
                        return Wildcard(field, value);
                    }
                    else
                    {
                        return Term(field, value);
                    }
                }                
            }

            return (isSeparated) ? MatchPhrases(field, values) : MatchPhrase(field, value);
        }

        protected string[] GetDynamicDateFormatsFromConfig(string key)
        {
            if (!Options.DynamicDateFormats.TryGetValue(key, out string[] dateFormats))
            {
                throw new InvalidOperationException($"Unable to find {key} date formats in configuration.");
            }

            return dateFormats;
        }

        private static bool IsSeparated(string value, string separator)
        {
            return !string.IsNullOrEmpty(separator) && value.Contains(separator);
        }

        private bool IsWildcard(string value)
        {
            return !string.IsNullOrEmpty(Options.WildcardStarMark) && value.Contains(Options.WildcardStarMark)
                || !string.IsNullOrEmpty(Options.WildcardQuestionMark) && value.Contains(Options.WildcardQuestionMark);
        }

        private static KeyValuePair<string, JsonNode> Exists(string field)
        {
            return new KeyValuePair<string, JsonNode>("exists", new JsonObject {["field"] = field});
        }

        private KeyValuePair<string, JsonNode>? Wildcard(string field, string value)
        {
            var actualValue = value;

            if (!string.IsNullOrEmpty(Options.WildcardStarMark))
            {
                actualValue = actualValue.Replace(Options.WildcardStarMark, "*");
            }

            if (!string.IsNullOrEmpty(Options.WildcardQuestionMark))
            {
                actualValue = actualValue.Replace(Options.WildcardQuestionMark, "?");
            }

            return new KeyValuePair<string, JsonNode>("wildcard", new JsonObject()
            {
                [field] = actualValue
            });
        }

        private KeyValuePair<string, JsonNode>? Wildcards(string field, string[] values)
        {
            return Should(values.Select(v => Wildcard(field, v)));
        }

        private static KeyValuePair<string, JsonNode> Term(string field, string value)
        {
            return new KeyValuePair<string, JsonNode>("term", new JsonObject()
            {
                [field] = value
            });
        }

        private static KeyValuePair<string, JsonNode> Terms(string field, string[] values)
        {
            return new KeyValuePair<string, JsonNode>("terms", new JsonObject()
            {
                [field] = new JsonArray(values.Select(n => JsonValue.Create(n)).ToArray())
            });
        }

        private static KeyValuePair<string, JsonNode> MatchPhrase(string field, string value)
        {
            return new KeyValuePair<string, JsonNode>("match_phrase", new JsonObject()
                {
                    [field] = new JsonObject()
                    {
                        ["query"] = value
                    }   
                }
            );
        }
        private static KeyValuePair<string, JsonNode> MatchPhrases(string field, string[] values)
        {
            var arr = values.Select(v => MatchPhrase(field, v))
                .Select(m => (JsonNode)new JsonObject {m}).ToArray();
            return new KeyValuePair<string, JsonNode>("bool", new JsonObject()
            {
                ["should"] = arr.Length > 1 ? new JsonArray(arr) : arr[0]
            });
        }

        private string[] GetFields(ProductsOptionsBase options)
        {
            string[] fields = null;

            if (options?.PropertiesFilter?.Any() == true)
            {
                fields = options.PropertiesFilter.ToArray();
            }
            else if (options?.Type == "RegionTags")
            {
                fields = new[] { "ProductId", "Type", "RegionTags" };
            }
            else if (Options.DefaultFields?.Any() == true)
            {
                fields = Options.DefaultFields;
            }
            return fields;
        }


        private bool IsNotAnalyzedField(string pathField)
        {
            return Options.NotAnalyzedFields.Any(mask =>
            {
                var field = pathField.Split('.').Last();

                bool startsWithMask = mask.StartsWith("*");
                bool endsWithMask = mask.EndsWith("*");
                string analyzedField = startsWithMask && endsWithMask ? mask.Replace("*", string.Empty) : mask;

                if (startsWithMask && endsWithMask)
                {
                    return field.Contains(analyzedField);
                }
                else if (startsWithMask)
                {
                    return field.EndsWith(analyzedField);
                }
                else if (endsWithMask)
                {
                    return field.StartsWith(analyzedField);
                }
                else
                {
                    return field.Equals(analyzedField);
                }
            });
        }

        #region IDisposable Support
        private bool _disposed;

        #endregion      
    }
}