using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Infrastructure;

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
        private const string DisableOrParameterName = "DisableOr";
        private const string DisableNotParameterName = "DisableNot";

        private IElasticConfiguration Configuration { get; }

        private SonicElasticStoreOptions Options { get; }

        public ElasticProductStore(IElasticConfiguration config, IOptions<SonicElasticStoreOptions> optionsAccessor)
        {
            Configuration = config;
            Options = optionsAccessor?.Value ?? new SonicElasticStoreOptions();
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
            var response = await client.UpdateAsync<JObject>(id, d => d.Upsert(product).Type(type));

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

                await client.DeleteIndexAsync(client.ConnectionSettings.DefaultIndex);

                await client.CreateIndexAsync(
                    client.ConnectionSettings.DefaultIndex,
                    index => index
                        .Settings(s => s.Setting("max_result_window", Options.MaxResultWindow))
                        .Mappings(m => m.MapNotAnalyzed(Options.Types, Options.NotAnalyzedFields))
                );

                return SonicResult.Success;
            }
            catch (Exception ex)
            {
                return SonicResult.Failed(SonicErrorDescriber.StoreFailure(ex.Message, ex));
            }
        }

        public async Task<Stream> GetProductsInTypeStreamAsync(string type, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            if (type == null) throw new ArgumentNullException(nameof(type));
            var q = JObject.FromObject(new
            {
                from = (options?.Page ?? 0) * (options?.PerPage ?? Options.DefaultSize),
                size = options?.PerPage ?? Options.DefaultSize,
                _source = new { include = GetFields(options, type) }
            });

            SetQuery(q, options);
            SetSorting(q, options);

            var client = Configuration.GetElasticClient(language, state);
            var response = await client.LowLevel.SearchAsync<Stream>(client.ConnectionSettings.DefaultIndex, type, q.ToString());
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


            var types = options?.Filters?
                .Where(f => f.Item1 == Options.TypePath)
                .Select(f => f.Item2)
                .FirstOrDefault();

            SetQuery(q, options);
            SetSorting(q, options);
            ElasticsearchResponse<Stream> response;
            var client = Configuration.GetElasticClient(language, state);
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
            var result = new List<JProperty>();

            if (productsOptions.Filters != null)
            {
                var disabledOrFields = GetFieldValues(productsOptions.Filters, DisableOrParameterName);
                var disabledNotFields = GetFieldValues(productsOptions.Filters, DisableNotParameterName);
                result.AddRange(productsOptions.Filters.Select(n => GetSingleFilterWithNot(n.Item1, n.Item2, disabledOrFields, disabledNotFields)));
            }

            if (productsOptions.RangeFilters != null)
            {
                result.Add(
                    new JProperty("range", new JObject(productsOptions.RangeFilters.Select(GetSingleRangeQuery)))
                );
            }

            if (productsOptions.Query != null)
            {
                result.Add(
                    new JProperty("query_string", JObject.FromObject(new { query = productsOptions.Query, lenient = true }))
                );
            }

            var obj = result.Select(n => new JObject(n)).ToArray();

            if (obj.Any())
            {
                var query = new JObject(
                    new JProperty("bool", new JObject(
                            new JProperty("must", obj.Length > 1 ? JArray.FromObject(obj) : (JToken)obj[0])
                        ))
                    );

                json.Add(new JProperty("query", query));
            }
        }

        private JProperty GetSingleRangeQuery(Tuple<string, string, string> elem)
        {
            var content = new JObject();
            if (elem.Item2 != "")
            {
                content.Add("gte", elem.Item2);
            }
            if (elem.Item3 != "")
            {
                content.Add("lte", elem.Item3);
            }
            return new JProperty(elem.Item1, content);
        }

        private void SetSorting(JObject json, ProductsOptions options)
        {
            if (!String.IsNullOrEmpty(options.Sort))
            {
                json.Add(new JProperty("sort", new JArray(new JObject(new JProperty(options.Sort, options.Order ? "asc" : "desc")))));
            }
        }

        private JProperty GetSingleFilterWithNot(string field, string value, string[] disabledOrFields, string[] disabledNotFields)
        {
            if (!string.IsNullOrEmpty(Options.NegationMark) && value.StartsWith(Options.NegationMark) && !disabledNotFields.Contains(field))
            {
                value = value.Remove(0, Options.NegationMark.Length);
                var condition = GetSingleFilter(field, value, Options.ValueSeparator);
                return new JProperty("bool", new JObject(
                    new JProperty("must_not", new JObject(condition))
                ));
            }
            else
            {
                var separator = disabledOrFields.Contains(field) ? null : Options.ValueSeparator;
                return GetSingleFilter(field, value, separator);
            }
        }

        private JProperty GetSingleFilter(string field, string value, string separator)
        {
            var isBaseField = field == Options.IdPath || field == ProductIdField;

            if (isBaseField)
            {
                if (value.Contains(BaseSeparator))
                {
                    var values = value.Split(new[] { BaseSeparator }, StringSplitOptions.None);
                    return Terms(field, values);
                }
                else if (!string.IsNullOrWhiteSpace(separator) && value.Contains(separator))
                {
                    var values = value.Split(new[] { separator }, StringSplitOptions.None);
                    return Terms(field, values);
                }
                else
                {
                    return Term(field, value);
                }
            }
            else
            {
                var notAnalized = IsNotAnalyzedField(field);
                var separated = !string.IsNullOrWhiteSpace(separator) && value.Contains(separator);
                var values = new string[0];

                if (separated)
                {
                    values = value.Split(new[] { separator }, StringSplitOptions.None);
                }

                if (notAnalized)
                {
                    if (separated)
                    {
                        return Terms(field, values);
                    }
                    else
                    {
                        return Term(field, value);
                    }
                }
                else
                {
                    if (separated)
                    {
                        var arr = values.Select(v => MatchPhrase(field, v)).Select(m => new JObject(m)).ToArray();
                        return new JProperty("bool", new JObject(
                            new JProperty("should", arr.Length > 1 ? (JArray.FromObject(arr)) : (JToken)arr[0])
                        ));
                    }
                    else
                    {
                        return MatchPhrase(field, value);
                    }
                }
            }
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
                    new JProperty("query", value),
                    new JProperty("operator", "and")
                )
            )));
        }

        private string[] GetFieldValues(IList<Tuple<string, string>> filters, string field)
        {
            var value = filters.FirstOrDefault(f => f.Item1 == field)?.Item2;

            if (string.IsNullOrEmpty(value))
            {
                return new string[0];
            }
            else
            {
                return value.Split(',');
            }
        }

        private string[] GetFields(ProductsOptions options, string type = null)
        {
            string[] fields = null;

            if (options?.PropertiesFilter?.Any() == true)
            {
                fields = options.PropertiesFilter.ToArray();
            }
            else if (type == "RegionTags")
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