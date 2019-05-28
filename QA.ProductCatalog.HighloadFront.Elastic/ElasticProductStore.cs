using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticProductStore : IProductStore
    {
        private const string BaseSeparator = ",";
        private const string ProductIdField = "ProductId";

        protected ElasticConfiguration Configuration { get; }

        protected SonicElasticStoreOptions Options { get; }

        private ILogger Logger { get; }

        static ElasticProductStore()
        {
            
        }
        
        public ElasticProductStore(ElasticConfiguration config, SonicElasticStoreOptions options, ILoggerFactory loggerFactory)
        {
            Configuration = config;
            Options = options;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public string GetId(JObject product)
        {

            if (product == null) throw new ArgumentNullException(nameof(product));

            return product[Options.IdPath]?.ToString();
        }

        public virtual string GetType(JObject product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            string type = product[Options.TypePath]?.ToString();
            if (type == null) return Options.DefaultType;
            return type;
        }

        public async Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> products, string language, string state)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));

            var failedResult = new List<SonicError>();
            var client = Configuration.GetElasticClient(language, state);
            var index = Configuration.GetElasticIndex(language, state);

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

                return $"{{\"index\":{{\"_index\":\"{index.Name}\",\"_type\":\"{type}\",\"_id\":\"{id}\"}}}}\n{json}\n";
            });

            if (failedResult.Any()) return SonicResult.Failed(failedResult.ToArray());
            try
            {

                var responseText = await client.BulkAsync(string.Join(string.Empty, commands));

                var responseJson = JObject.Parse(responseText);

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

                return SonicResult.Success;
            }
            catch (Exception ex)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(ex.Message, ex));
            }
        }

        public async Task<string> FindByIdAsync(ProductsOptions options, string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            return await client.FindSourceByIdAsync(options.Id.ToString(), options?.PropertiesFilter?.ToArray());
        }


        public async Task<SonicResult> CreateAsync(JObject product, string language, string state)
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
            var json = JsonConvert.SerializeObject(product);
            try
            {
                await client.PutAsync(id, type, json);
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }

        }

        public async Task<SonicResult> UpdateAsync(JObject product, string language, string state)
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
            
            var json = JsonConvert.SerializeObject(product);
            var client = Configuration.GetElasticClient(language, state);

            try
            {
                await client.UpdateAsync(id, type, json);
                return SonicResult.Success;
            }
            catch (Exception e)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(e.Message, e));
            }
        }

        public async Task<SonicResult> DeleteAsync(JObject product, string language, string state)
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

        public async Task<bool> Exists(JObject product, string language, string state)
        {
            string id = GetId(product);
            string type = GetType(product);
            var client = Configuration.GetElasticClient(language, state);
            return await client.DocumentExistsAsync(id, type);
            
        }

        public async Task<SonicResult> ResetAsync(string language, string state)
        {
            var client = Configuration.GetElasticClient(language, state);
            try
            {
                if (await client.IndexExistsAsync())
                {
                    await client.DeleteIndexAsync();
                }

                await client.CreateIndexAsync(GetDefaultIndexSettings().ToString());
                
                return SonicResult.Success;
            }
            catch (Exception ex)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(ex.Message, ex));
            }
        }

        protected virtual JObject GetDefaultIndexSettings()
        {
            var indexSettings = new JObject(
                new JProperty("settings", new JObject(
                    new JProperty("max_result_window", Options.MaxResultWindow),
                    new JProperty("mapping.total_fields.limit", Options.TotalFieldsLimit)
                )),
                new JProperty("mappings", GetMappings(Options.Types, Options.NotAnalyzedFields))
            );
            return indexSettings;
        }

        public virtual async Task<string> SearchAsync(ProductsOptions options, string language, string state)
        {
            var q = GetQuery(options).ToString();
            var client = Configuration.GetElasticClient(language, state);
            return await client.SearchAsync(options.ActualType, q);
        }

        protected JObject GetQuery(ProductsOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var q = JObject.FromObject(new
            {
                from = options.ActualFrom,
                size = options.ActualSize,
                _source = new {include = GetFields(options)}
            });

            SetQuery(q, options);
            SetSorting(q, options);
            return q;
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
            var response = await client.SearchAsync(null, q.ToString());
            return JObject.Parse(response).SelectTokens("aggregations.typesAgg.buckets.[?(@.key)].key").Select(n => n.ToString())
                .ToArray();
        }   

        protected JObject GetKeywordTemplate(string type, string field)
        {
            return new JObject(
                new JProperty($"not_analyzed_{type}_{field}", new JObject(
                    new JProperty("match_mapping_type", "string"),
                    new JProperty("match", field),
                    new JProperty("mapping", new JObject(
                        new JProperty("type", "keyword")
                    ))
                ))
            );
        }

        protected virtual JObject GetMapping(string type, string[] fields)
        {
            var templates = new JArray(fields.Select(n => GetKeywordTemplate(type, n)));

            return new JObject(
                new JProperty(type, new JObject(
                    new JProperty("dynamic_templates", templates)                   
                ))
            );
        }

        public JObject GetMappings(string[] types, string[] fields)
        {
            var result = new JObject();
            foreach (var type in types)
            {
                result.Add(new JProperty(type, GetMapping(type, fields)));
            }
            return result;
        }

        protected virtual void SetQuery(JObject json, ProductsOptions productsOptions)
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

        protected JProperty CreateQueryElem(IElasticFilter filter, string[] disableOr, string[] disableNot, string[] disableLike)
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
                    ? GetSingleFilterWithNot(simpleFilter.Name, simpleFilter.Values, simpleFilter.FromJson, disableOr, disableNot, disableLike)
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

        protected static JProperty Must(IEnumerable<JProperty> props)
        {
            return Bool(props, false);
        }

        protected static JProperty Should(IEnumerable<JProperty> props)
        {
            return Bool(props, true);
        }

        protected static JProperty Bool(IEnumerable<JProperty> props, bool isDisjunction)
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

        private JProperty GetSingleFilterWithNot(string field, StringValues values, bool fromJson, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            var conditions = StringValues.IsNullOrEmpty(values)
                ? null
                : values.Select(n => GetSingleFilterWithNot(field, n, disabledOrFields, disabledNotFields, disableLikeFields)).ToArray();

            if (conditions == null || conditions.Length == 0)
                return null;
            
            var op = (fromJson) ? "should" : "must";

            var result = conditions.Length == 1 ? 
                conditions.First() : 
                new JProperty("bool", new JObject(new JProperty(op, JArray.FromObject(conditions.Select(n => new JObject(n))))));

            return result;
        }

        private JProperty GetSingleFilterWithNot(string field, string value, string[] disabledOrFields, string[] disabledNotFields, string[] disableLikeFields)
        {
            JProperty result;
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

        protected JProperty GetSingleFilter(string field, string value, string separator, bool  disableLike)
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

        public string GetJsonByAlias(string alias)
        {
            return Configuration.GetJsonByAlias(alias);
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