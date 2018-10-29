using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.Logger;
using QA.ProductCatalog.HighloadFront.Elastic.Extensions;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore : 
        IProductTypeStore,
        IProductBulkStore,
        IProductStreamStore,
        IProductSearchStore
    {
        private const string BaseSeparator = ",";
        private const string ProductIdField = "ProductId";

        private IElasticConfiguration Configuration { get; }

        private SonicElasticStoreOptions Options { get; }

        private ILogger Logger { get; }

        static ElasticProductStore()
        {
            ServicePointManager.DefaultConnectionLimit = ConnectionConfiguration.DefaultConnectionLimit;
        }
        
        public ElasticProductStore(IElasticConfiguration config, IOptions<SonicElasticStoreOptions> optionsAccessor, ILogger logger)
        {
            Configuration = config;
            Options = optionsAccessor?.Value ?? new SonicElasticStoreOptions();
            Logger = logger;
        }

        public string GetId(JObject product)
        {
            ThrowIfDisposed();

            if (product == null) throw new ArgumentNullException(nameof(product));

            return product[Options.IdPath]?.ToString();
        }

        public string GetType(JObject product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            string type = product[Options.TypePath]?.ToString();

            if (type == null) return Options.DefaultType;

            return type;
        }

        public async Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> products, string language, string state)
        {
            ThrowIfDisposed();

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

                var json = JsonConvert.SerializeObject(p);

                return $"{{\"index\":{{\"_index\":\"{client.ConnectionSettings.DefaultIndex}\",\"_type\":\"{type}\",\"_id\":\"{id}\"}}}}\n{json}\n";
            });

            if (failedResult.Any()) return SonicResult.Failed(failedResult.ToArray());

            try
            {
                var response = await client.LowLevel.BulkAsync<VoidResponse>(string.Join(string.Empty, commands));

                var responseText = System.Text.Encoding.UTF8.GetString(response.ResponseBodyInBytes);
                var responseJson = JObject.Parse(responseText);

                if (response.Success)
                {

                    if (responseJson.Value<bool>("errors"))
                    {
                        var errors = responseJson["items"]
                            .Select(i => i["index"]?["error"])
                            .Where(i => i != null)
                            .Select(i => new
                            {
                                Code = i.Value<string>("type"),
                                Description = i.Value<string>("reason")
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
                    else
                    {
                        return SonicResult.Success;
                    }
                }
                else
                {
                    return SonicResult.Failed(SonicErrorDescriber.StoreFailure(response.OriginalException.Message));
                }
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }

        public async Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(string id, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            var client = Configuration.GetElasticClient(language, state); 
            var response = await client.LowLevel.GetSourceAsync<Stream>(client.ConnectionSettings.DefaultIndex, "_all", id, p =>
            {
                if (options?.PropertiesFilter != null)
                {
                    p.SourceInclude(options.PropertiesFilter.ToArray());
                }
                return p;
            });

            return response;
        }


        public async Task<SonicResult> CreateAsync(JObject product, string language, string state)
        {
            ThrowIfDisposed();

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

            var json = JsonConvert.SerializeObject(product);

            try
            {
                var client = Configuration.GetElasticClient(language, state); 
                var response = await client.LowLevel.IndexAsync<VoidResponse>(client.ConnectionSettings.DefaultIndex, type, id, json);
                return response.Success
                    ? SonicResult.Success
                    : SonicResult.Failed(SonicErrorDescriber
                        .StoreFailure(response.OriginalException.Message));
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }

        }

        public async Task<SonicResult> UpdateAsync(JObject product, string language, string state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

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
            var response = await client.UpdateAsync<JObject>(id, d => d.Upsert(product).Type(type), cancellationToken);

            return response.IsValid
                ? SonicResult.Success
                : SonicResult.Failed(SonicErrorDescriber
                    .StoreFailure(response.OriginalException.Message));
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
        {
            ThrowIfDisposed();

            string id = GetId(product);
            string type = GetType(product);

            try
            {
                var client = Configuration.GetElasticClient(language, state);

                var request = new DeleteRequest(client.ConnectionSettings.DefaultIndex, type, id);

                var response = await client.DeleteAsync(request);

                return response.IsValid
                    ? SonicResult.Success
                    : SonicResult.Failed(SonicErrorDescriber.StoreFailure(response.OriginalException.Message));
            }
            catch (ElasticsearchClientException ex) when (ex.Response.HttpStatusCode == 404)
            {
                return SonicResult.Success;
            }
        }

        public async Task<bool> Exists(JObject product, string language, string state)
        {
            ThrowIfDisposed();

            string id = GetId(product);
            string type = GetType(product);

            var client = Configuration.GetElasticClient(language, state);
            var existsRequest = new DocumentExistsRequest(client.ConnectionSettings.DefaultIndex, type, id);
            var existsResponse = await client.DocumentExistsAsync(existsRequest);

            return existsResponse.Exists;
        }

        public async Task<SonicResult> ResetAsync(string language, string state)
        {
            ThrowIfDisposed();

            try
            {
                var client = Configuration.GetElasticClient(language, state);

                var indexResult = await client.IndexExistsAsync(client.ConnectionSettings.DefaultIndex);

                if (indexResult == null || !indexResult.IsValid)
                {
                    return SonicResult.Failed(SonicErrorDescriber.StoreFailure("index is not available"));
                }
                else if (indexResult.Exists)
                {
                    await client.DeleteIndexAsync(client.ConnectionSettings.DefaultIndex);
                }

                await client.CreateIndexAsync(
                    client.ConnectionSettings.DefaultIndex,
                    index => index
                        .Settings(s => s
                            .Setting("max_result_window", Options.MaxResultWindow)
                            .Setting("mapping.total_fields.limit", Options.TotalFieldsLimit)
                        )
                        .Mappings(m => m.MapNotAnalyzed(Options.Types, Options.NotAnalyzedFields))
                );

                return SonicResult.Success;
            }
            catch (Exception ex)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(ex.Message, ex));
            }
        }

        public async Task<Stream> GetProductsInTypeStreamAsync(ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var size = options.Take ?? options.PerPage ?? Options.DefaultSize;
            var from = options.Skip ?? (options.Page ?? 0) * size;
            
            var q = JObject.FromObject(new
            {
                from,
                size,
                _source = new { include = GetFields(options) }
            });

            SetQuery(q, options);
            SetSorting(q, options);

            var client = Configuration.GetElasticClient(language, state);
#if DEBUG            
            var timer = new Stopwatch();
            timer.Start();
#endif
            var response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, options.Type, q.ToString());
#if DEBUG             
            timer.Stop();
            Logger.Debug("Query to ElasticSearch took {0} ms", timer.Elapsed.TotalMilliseconds);
#endif
            return response.Body;

        }

        public async Task<Stream> SearchStreamAsync(ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            var q = JObject.FromObject(new
            {
                from = (options?.Page ?? 0) * (options?.PerPage ?? Options.DefaultSize),
                size = options?.PerPage ?? Options.DefaultSize,
                _source = new { include = GetFields(options) }
            });

            SetQuery(q, options);
            SetSorting(q, options);

            var client = Configuration.GetElasticClient(language, state);
            ElasticsearchResponse<Stream> response;
            var types = GetTypesString(options);
            if (types == null)
            {
                response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, q.ToString());
            }
            else
            {
                response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, types, q.ToString());
            }
            return response.Body;
        }

        private string GetTypesString(ProductsOptions options)
        {
            var types = options?.SimpleFilters?
                .Where(f => f.Name == Options.TypePath)
                .Select(f => f.Value).FirstOrDefault();

            return types;
        }

        public async Task<string[]> GetTypesAsync(string language, string state)
        {
            var q = new JObject(
                new JProperty("aggs", new JObject(
                    new JProperty("typesAgg", new JObject(
                        new JProperty("terms", new JObject(
                            new JProperty("field", "_type"),
                            new JProperty("size", "200")
                        ))
                    ))
                )),
                new JProperty("size", 0)
            );
            var client = Configuration.GetElasticClient(language, state);
            var response = await client.LowLevel.SearchAsync<JObject>(client.ConnectionSettings.DefaultIndex, q.ToString());
            return response.Body.SelectTokens("aggregations.typesAgg.buckets.[?(@.key)].key").Select(n => n.ToString())
                .ToArray();
        }

        private void SetQuery(JObject json, ProductsOptions productsOptions)
        {
            JProperty query = null;
            var filters = productsOptions.Filters;
            if (filters != null)
            {
                var conditions = filters.Select(n => CreateQueryElem(n, productsOptions.DisableOr, productsOptions.DisableNot, productsOptions.DisableLike));
                var shouldGroups = new List<List<JProperty>>();
                var currentGroup = new List<JProperty>();
                foreach (var condition in conditions)
                {
                    if (condition == null)
                        continue;

                    if (condition.Value["or"] != null)
                    {
                        if (currentGroup.Any())
                        {
                            shouldGroups.Add(currentGroup);
                        }
                        condition.Value["or"].Parent.Remove();
                        currentGroup = new List<JProperty>();
                    }

                    currentGroup.Add(condition);
                }
                shouldGroups.Add(currentGroup);

                query = shouldGroups.Count == 1 ? Must(currentGroup) : Should(shouldGroups.Select(Must));
            }

            if (query != null)
            {
                json.Add(new JProperty("query", new JObject(query)));
            }
        }

        private JProperty CreateQueryElem(IElasticFilter filter, string[] disableOr, string[] disableNot, string[] disableLike)
        {
            var simpleFilter = filter as SimpleFilter;
            var rangeFilter = filter as RangeFilter;
            var queryFilter = filter as QueryFilter;
            var groupFilter = filter as GroupFilter;
            
            JProperty result = null;

            if (groupFilter != null)
            {
                var props = groupFilter.Filters.Select(n =>
                    CreateQueryElem(n, disableOr, disableNot, disableLike)).ToArray();

                return (groupFilter.IsDisjunction) ? Should(props) : Must(props);
            }

            if (simpleFilter != null)
            {
                result = simpleFilter.Name != Options.TypePath
                    ? GetSingleFilterWithNot(simpleFilter.Name, simpleFilter.Values, disableOr, disableNot, disableLike)
                    : null;
            }

            if (rangeFilter != null)
            {
                result = new JProperty("range", 
                    new JObject(GetSingleRangeQuery(rangeFilter))
                );
            }

            if (queryFilter != null)
            {
                result = new JProperty("query_string",
                    JObject.FromObject(new {query = queryFilter.Query, lenient = true})
                );
            }

            if (filter.IsDisjunction && result != null)
            {
                result.Value["or"] = true;
            }

            return result;
        }

        private static JProperty Must(IEnumerable<JProperty> props)
        {
            return Bool(props, false);
        }

        private static JProperty Should(IEnumerable<JProperty> props)
        {
            return Bool(props, true);
        }

        private static JProperty Bool(IEnumerable<JProperty> props, bool isDisjunction)
        {
            if (props == null) return null;
            var obj = props.Where(n => n != null).Select(n => new JObject(n)).ToArray();
            if (!obj.Any()) return null;

            return new JProperty("bool", new JObject(
                new JProperty(isDisjunction ? "should" : "must", obj.Length > 1 ? JArray.FromObject(obj) : (JToken)obj[0])
            ));
        }

        private JProperty GetSingleRangeQuery(RangeFilter elem)
        {
            var content = new JObject();
            if (elem.Floor != "")
            {
                content.Add("gte", elem.Floor);
            }
            if (elem.Ceiling != "")
            {
                content.Add("lte", elem.Ceiling);
            }
            return new JProperty(elem.Name, content);
        }

        private void SetSorting(JObject json, ProductsOptions options)
        {
            if (!String.IsNullOrEmpty(options.Sort))
            {
                json.Add(new JProperty("sort", new JArray(new JObject(new JProperty(options.Sort, options.DirectOrder ? "asc" : "desc")))));
            }
        }

        private JProperty GetSingleFilterWithNot(string field, StringValues values, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            var conditions = StringValues.IsNullOrEmpty(values)
                ? null
                : values.Select(n => GetSingleFilterWithNot(field, n, disabledOrFields, disabledNotFields, disableLikeFields)).ToArray();

            if (conditions == null || conditions.Length == 0)
                return null;

            var result = conditions.Length == 1 ? 
                conditions.First() : 
                new JProperty("bool", new JObject(new JProperty("must", JArray.FromObject(conditions.Select(n => new JObject(n))))));

            return result;
        }

        private JProperty GetSingleFilterWithNot(string field, string value, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            JProperty result;
            var actualSeparator = GetActualSeparator(field, disabledOrFields);
            var disableLike = disableLikeFields.Contains(field);
            var actualValue = GetActualValue(field, value, disabledNotFields, out var hasNegation);

            if (actualValue == "null")
            {
                result = Exists(field);
                hasNegation = !hasNegation;
            }
            else
            {
                result = GetSingleFilter(field, actualValue, actualSeparator, disableLike);  
            }

            if (hasNegation)
            {
                result = new JProperty("bool", new JObject(
                    new JProperty("must_not", new JObject(result))
                ));
            }



            return result;
        }

        private string GetActualValue(string field, string value, string[] disabledNotFields, out bool hasNegation)
        {
            hasNegation = !string.IsNullOrEmpty(Options.NegationMark)
                          && (value.StartsWith(Options.NegationMark))
                          && !disabledNotFields.Contains(field);

            return (hasNegation) ? value.Substring(Options.NegationMark.Length) : value;
        }

        private string GetActualSeparator(string field, string[] disabledOrFields)
        {
            var actualSeparator = !string.IsNullOrWhiteSpace(Options.ValueSeparator) ? Options.ValueSeparator : BaseSeparator;
            actualSeparator = disabledOrFields.Contains(field) ? null : actualSeparator;
            return actualSeparator;
        }

        private JProperty GetSingleFilter(string field, string value, string separator, bool  disableLike)
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

        private static bool IsSeparated(string value, string separator)
        {
            return !string.IsNullOrEmpty(separator) && value.Contains(separator);
        }

        private bool IsWildcard(string value)
        {
            return !string.IsNullOrEmpty(Options.WildcardStarMark) && value.Contains(Options.WildcardStarMark)
                || !string.IsNullOrEmpty(Options.WildcardQuestionMark) && value.Contains(Options.WildcardQuestionMark);
        }

        private static JProperty Exists(string field)
        {
            return new JProperty("exists", new JObject(new JProperty("field", field)));
        }

        private JProperty Wildcard(string field, string value)
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

            return new JProperty("wildcard", new JObject(new JProperty(field, actualValue)));
        }

        private JProperty Wildcards(string field, string[] values)
        {
            return Should(values.Select(v => Wildcard(field, v)));
        }

        private static JProperty Term(string field, string value)
        {
            return new JProperty("term", new JObject(new JProperty(field, value)));
        }

        private static JProperty Terms(string field, string[] values)
        {
            return new JProperty("terms", new JObject(new JProperty(field, JArray.FromObject(values))));
        }

        private static JProperty MatchPhrase(string field, string value)
        {
            return new JProperty("match_phrase", new JObject(new JProperty(field,
                new JObject(
                    new JProperty("query", value)
                )
            )));
        }
        private static JProperty MatchPhrases(string field, string[] values)
        {
            var arr = values.Select(v => MatchPhrase(field, v)).Select(m => new JObject(m)).ToArray();
            return new JProperty("bool", new JObject(
                new JProperty("should", arr.Length > 1 ? (JArray.FromObject(arr)) : (JToken)arr[0])
            ));
        }

        private string[] GetFields(ProductsOptions options)
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