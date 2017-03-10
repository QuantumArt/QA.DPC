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
        IProductStore,
        IProductTypeStore,
        IProductBulkStore,
        IProductStreamStore,
        IProductSearchStore
    {
        private const string BaseSeparator = ",";
        private const string ProductIdField = "ProductId";

        private IElasticClient _client { get; }
        private Func<string, string, IElasticClient> _getClient { get; }

        private SonicElasticStoreOptions Options { get; set; }

        public ElasticProductStore(IElasticClient client, Func<string, string, IElasticClient> getClient, IOptions<SonicElasticStoreOptions> optionsAccessor)
        {
            _client = client;
            _getClient = getClient;
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
            var client = _getClient(language, state);

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

        public async Task<ElasticsearchResponse<string>> FindByIdAsync(string id, ProductsOptions options)
        {
            ThrowIfDisposed();

            var response = await _client.LowLevel.GetSourceAsync<string>(_client.ConnectionSettings.DefaultIndex, "_all", id, p =>
            {
                if (options?.PropertiesFilter != null)
                {
                    p.SourceInclude(options.PropertiesFilter.ToArray());
                }
                return p;
            });

            return response;
        }

        public async Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(string id, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            var client = _getClient(language, state);
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
                var client = _getClient(language, state);
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

            var client = _getClient(language, state);
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
                var request = new DeleteRequest("products", type, id);
                var client = _getClient(language, state);
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

            var existsRequest = new DocumentExistsRequest(_client.ConnectionSettings.DefaultIndex, type, id);
            var existsResponse = await _client.DocumentExistsAsync(existsRequest);

            return existsResponse.Exists;
        }

        public async Task<SonicResult> ResetAsync(string language, string state)
        {
            ThrowIfDisposed();

            try
            {
                var client = _getClient(language, state);

                var response = await client.DeleteIndexAsync(client.ConnectionSettings.DefaultIndex);

                var r = await client.CreateIndexAsync(
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

        public async Task<IList<JObject>> GetProductsInTypeAsync(string type, ProductsOptions options)
        {
            ThrowIfDisposed();

            if (type == null) throw new ArgumentNullException(nameof(type));

            var request = new SearchDescriptor<JObject>()
                .Type(type)
                .From((options?.Page ?? 0) * (options?.PerPage ?? Options.DefaultSize))
                .Size(options?.PerPage ?? Options.DefaultSize)
                .Query(q => Filter(q, options));

            //var filter = MakeFilter(options);
            //request.Filter(filter);

            if (options?.PropertiesFilter?.Any() == true)
                request.Source(s => s.Include(f => f.Fields(options.PropertiesFilter.ToArray())));

            var response = await _client.SearchAsync<JObject>(request);

            return response.Documents.ToList();
        }

        public async Task<Stream> GetProductsInTypeStreamAsync(string type, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            if (type == null) throw new ArgumentNullException(nameof(type));

            var request = new SearchDescriptor<StreamData>()
                .Type(type)
                .From((options?.Page ?? 0) * (options?.PerPage ?? Options.DefaultSize))
                .Size(options?.PerPage ?? Options.DefaultSize)
                .Query(q => Filter(q, options));

            SetPropertyFilter(request, options, type);
            SetSorting(request, options);

            var client = _getClient(language, state);
            return await client.SearchStreamAsync(request);
        }

        private QueryContainer Filter(QueryContainer query, ProductsOptions productsOptions)
        {
            if (productsOptions == null) return query;

            if (productsOptions.Filters != null)
            {
                query = productsOptions.Filters.Where(f => f.Item1 != Options.TypePath).Aggregate(query, (current, sf) =>
                {
                    var field = sf.Item1;
                    var value = sf.Item2;
                    var separator = Options.ValueSeparator;
                    var isBaseField = field == Options.IdPath || field == ProductIdField;

                    if (isBaseField)
                    {

                        if (value.Contains(BaseSeparator))
                        {
                            var values = value.Split(new string[] { BaseSeparator }, StringSplitOptions.None);

                            current = current & +new TermsQuery
                            {
                                Field = field,
                                Terms = values
                            };
                        }
                        else if (!string.IsNullOrWhiteSpace(separator) && value.Contains(separator))
                        {
                            var values = value.Split(new string[] { separator }, StringSplitOptions.None);

                            current = current & +new TermsQuery
                            {
                                Field = field,
                                Terms = values
                            };
                        }
                        else
                        {
                            current = current & +new TermQuery
                            {
                                Field = field,
                                Value = value
                            };
                        }
                    }                 
                    else
                    {                        
                        var notAnalized = IsNotAnalyzedField(field);
                        var separated = !string.IsNullOrWhiteSpace(separator) && value.Contains(separator);
                        var values = new string[0];

                        if (separated)
                        {
                            values = value.Split(new string[] { separator }, StringSplitOptions.None);
                        }

                        if (notAnalized)
                        {
                            if (separated)
                            {
                                current = current & +new TermsQuery
                                {
                                    Field = field,
                                    Terms = values
                                };
                            }
                            else
                            {
                                current = current & +new TermQuery
                                {
                                    Field = field,
                                    Value = value
                                };
                            }
                        }
                        else
                        {
                            if (separated)
                            {
                                current = current & +new BoolQuery
                                {
                                    Should = values.Select(v => (QueryContainer)
                                        new MatchPhraseQuery
                                        {
                                            Field = field,
                                            Query = v,
                                            Operator = Operator.And
                                        })
                                };
                            }
                            else
                            {
                                current = current & +new MatchPhraseQuery
                                {
                                    Field = field,
                                    Query = value,
                                    Operator = Operator.And
                                };
                            }
                        }                      
                    }

                    return current;
                });
            }

            if (productsOptions.RangeFilters != null)
            {
                query = productsOptions.RangeFilters.Aggregate(query, (current, rf) =>
                current & new TermRangeQuery
                {
                    Field = rf.Item1,
                    GreaterThanOrEqualTo = rf.Item2 == "" ? null : rf.Item2,
                    LessThanOrEqualTo = rf.Item3 == "" ? null : rf.Item3,
                });
            }

            if (productsOptions.Query != null)
            {
                query &= new QueryStringQuery
                {
                    Query = productsOptions.Query,
                    Lenient = true,
                };
            }

            return query;
        }

        private void SetPropertyFilter(SearchDescriptor<StreamData> request, ProductsOptions options, string type = null)
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

            if (fields != null)
            {
                request.Source(s => s.Include(f => f.Fields(fields)));
            }
        }

        private void SetSorting(SearchDescriptor<StreamData> request, ProductsOptions options)
        {
            if (!string.IsNullOrEmpty(options.Sort))
            {
                if (options.Order)
                {
                    request.Sort(d => d.Ascending(options.Sort));
                }
                else
                {
                    request.Sort(d => d.Descending(options.Sort));
                }
            }
        }

        /*
        private static FilterContainer MakeFilter(ProductsOptions productsOptions)
        {
            FilterContainer filter = new MatchAllFilter();

            if (productsOptions == null) return filter;

            if (productsOptions.Filters != null)
            {
                filter = productsOptions.Filters.Aggregate(filter, (current, sf) =>
                current & new TermFilter
                {
                    Field = sf.Item1,
                    Value = sf.Item2
                });
            }

            if (productsOptions.RangeFilters != null)
            {
                filter = productsOptions.RangeFilters.Aggregate(filter, (current, rf) =>
                current & new RangeFilter
                {
                    Field = rf.Item1,
                    GreaterThanOrEqualTo = rf.Item2.ToString(),
                    LowerThanOrEqualTo = rf.Item3.ToString(),
                });
            }

            if (productsOptions.Query != null)
            {
                filter &= new QueryFilter
                {
                    Query = new QueryStringQuery
                    {
                        Query = productsOptions.Query, Lenient = true
                    }.ToContainer()
                };
            }

            return filter;
        }
        */

        public async Task<IList<JObject>> SearchAsync(string q, ProductsOptions options)
        {
            ThrowIfDisposed();
            //var filter = MakeFilter(options);
            var respons = await _client.SearchAsync<JObject>(body => body
                //.Filter(filter)
                .Query(query => query.QueryString(qs => qs.Query(q).Lenient()))
                .AllTypes());

            return respons.Documents.ToList();
        }

        public async Task<Stream> SearchStreamAsync(string q, ProductsOptions options, string language, string state)
        {
            ThrowIfDisposed();

            var request = new SearchDescriptor<StreamData>()
                 .From((options?.Page ?? 0) * (options?.PerPage ?? Options.DefaultSize))
                 .Size(options?.PerPage ?? Options.DefaultSize)
                 .Query(query => Filter(query, options) & query.QueryString(qs => qs.Query(q).Lenient()));

            var types = options?.Filters?
                .Where(f => f.Item1 == Options.TypePath)
                .Select(f => f.Item2)
                .FirstOrDefault();

            if (types == null)
            {
                request.AllTypes();
            }
            else
            {
                request.Type(types);
            }

            SetPropertyFilter(request, options);
            SetSorting(request, options);

            var client = _getClient(language, state);
            return await client.SearchStreamAsync(request);
        }

        private bool IsNotAnalyzedField(string pathField)
        {
            return Options.NotAnalyzedFields.Any(mask => {
                var field = pathField.Split(new[] { '.' }).Last();

                bool startsWithMask = mask.StartsWith("*");
                bool endsWithMask = mask.EndsWith("*");
                string analyzedField = startsWithMask && endsWithMask ? analyzedField = mask.Replace("*", string.Empty) : mask;

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