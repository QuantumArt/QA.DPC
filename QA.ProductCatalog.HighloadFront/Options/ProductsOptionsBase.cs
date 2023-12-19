using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QA.ProductCatalog.HighloadFront.Constants;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public abstract class ProductsOptionsBase
    {
        private static readonly HashSet<string> FirstLevelReservedKeywords = new HashSet<string>()
        {
            HighloadParams.Page,
            HighloadParams.PerPage,
            HighloadParams.Skip,
            HighloadParams.Take,
            HighloadParams.Fields,
            HighloadParams.Sort,
            HighloadParams.Order,
            HighloadParams.DisableOr,
            HighloadParams.DisableNot,
            HighloadParams.DisableLike,
            HighloadParams.Query,
            HighloadParams.DataFilters,
            HighloadParams.CacheForSeconds,
            HighloadParams.Expand,
            HighloadParams.Path,
            HighloadParams.Name
        };

        private static readonly Regex RangeFilterRegex = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");
        private static readonly Regex GetRequestExpandParamRegex = new Regex(@$"^{HighloadParams.Expand}\[\d+\]\.");

        protected JsonElement _json;

        protected ProductsOptionsBase()
        {
            ElasticOptions = new SonicElasticStoreOptions();
            Filters = new List<IElasticFilter>();
            DataFilters = new Dictionary<string, string>();
        }

        public SonicElasticStoreOptions ElasticOptions { get; set; }

        #region Bound properties

        [ModelBinder(Name = "type")]
        public string Type { get; set; }
        
        [ModelBinder(Name = HighloadParams.Id)]
        public int Id { get; set; }
        
        [ModelBinder(Name = HighloadParams.Fields)]
        public string Fields { get; set; }
        
        [ModelBinder(Name = HighloadParams.Page)]
        public decimal? Page { get; set; }
        
        [ModelBinder(Name = HighloadParams.PerPage)]
        public decimal? PerPage { get; set; }
        
        [ModelBinder(Name = HighloadParams.Skip)]
        public decimal? Skip { get; set; }

        [ModelBinder(Name = HighloadParams.Take)]
        public decimal? Take { get; set; }

        [ModelBinder(Name = HighloadParams.Sort)]
        public string Sort { get; set; }

        [ModelBinder(Name = HighloadParams.Order)]
        public string OrderDirection { get; set; }

        [ModelBinder(Name = HighloadParams.DisableOr)]
        public string[] DisableOr { get; set; }

        [ModelBinder(Name = HighloadParams.DisableNot)]
        public string[] DisableNot { get; set; }

        [ModelBinder(Name = HighloadParams.DisableLike)]
        public string[] DisableLike { get; set; }

        [ModelBinder(Name = HighloadParams.Expand)]
        public ProductsOptionsExpand[] Expand { get; set; }

        #endregion

        #region Computed properties

        [BindNever]
        public IList<IElasticFilter> Filters { get; set; }
        
        [BindNever]
        public IList<string> PropertiesFilter { get; set; }

        [BindNever]
        public Dictionary<string, string> DataFilters { get; set; }
        
        [BindNever]     
        public decimal CacheForSeconds { get; set; }

        [BindNever]    
        public IList<SimpleFilter> SimpleFilters => Filters.OfType<SimpleFilter>().ToList();

        [BindNever]    
        public bool DirectOrder => OrderDirection == "asc";

        [BindNever]    
        public decimal ActualSize => Take ?? PerPage ?? ElasticOptions.DefaultSize;

        [BindNever]    
        public decimal ActualFrom => (Id != 0) ? 0 : Skip ?? (Page ?? 0) * ActualSize;
        
        [BindNever]    
        public string ActualType
        {
            get
            {
                if (!string.IsNullOrEmpty(Type))
                {
                    return Type;
                }

                return SimpleFilters
                    .Where(f => f.Name == ElasticOptions.TypePath)
                    .Select(f => f.Value)
                    .FirstOrDefault();
            }
        }

        #endregion

        public T BuildFromJson<T>(JsonElement json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
            where T : ProductsOptionsBase
        {
            return (T)BuildFromJson(json, options, id, skip, take);
        }

        public void ApplyQueryCollection(IQueryCollection collection)
        {
            Filters = collection
                .Where(x => !FirstLevelReservedKeywords.Contains(x.Key) && !GetRequestExpandParamRegex.IsMatch(x.Key))
                .Select(x => ProductOptionsParser.CreateFilter(x, ElasticOptions))
                .ToArray();
        }

        public virtual string GetKey()
        {
            return $"Id: {Id}, Skip: {Skip}, Take: {Take} {_json.ToString()}";
        }



        protected virtual ProductsOptionsBase BuildFromJson(JsonElement json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
        {
            ElasticOptions = options ?? new SonicElasticStoreOptions();
            Id = id ?? 0;

            if (json.ValueKind == JsonValueKind.Object)
            {
                _json = json;
                Page = GetJsonNullableDecimal(HighloadParams.Page);
                PerPage = GetJsonNullableDecimal(HighloadParams.PerPage);
                Skip = skip ?? GetJsonNullableDecimal(HighloadParams.Skip);
                Take = take ?? GetJsonNullableDecimal(HighloadParams.Take);
                CacheForSeconds = GetJsonNullableDecimal(HighloadParams.CacheForSeconds) ?? 0;

                PropertiesFilter = GetJsonStringArray(HighloadParams.Fields);
                DisableOr = GetJsonStringArray(HighloadParams.DisableOr);
                DisableNot = GetJsonStringArray(HighloadParams.DisableNot);
                DisableLike = GetJsonStringArray(HighloadParams.DisableLike);

                Sort = GetJsonString(HighloadParams.Sort);
                OrderDirection = GetJsonString(HighloadParams.Order);

                Filters = GetFilters(HighloadParams.Query);
                DataFilters = GetDataFilters(HighloadParams.DataFilters);
                Expand = GetExpand(HighloadParams.Expand);
            }
            
            return this;
        }

        private Dictionary<string, string> GetDataFilters(string name)
        {
            if (_json.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object)
            {
                return value.EnumerateObject().ToDictionary(k => k.Name, v => v.Value.ToString());
            }
            return new Dictionary<string, string>();
        }

        private IList<IElasticFilter> GetFilters(string name)
        {
            var result = new List<IElasticFilter>();
            List<JsonProperty> filterProperties;

            if (_json.TryGetProperty(name, out var filters) && filters.ValueKind == JsonValueKind.Object)
            {
                filterProperties = filters.EnumerateObject().ToList();
            }
            else
            {
                filterProperties = _json.EnumerateObject()
                    .Where(n => !FirstLevelReservedKeywords.Contains(n.Name))
                    .ToList();
            }
            
            if (Id != 0)
            {
                using var doc = JsonDocument.Parse($@"{{""Id"": {Id}}}");
                filterProperties.Add(doc.RootElement.Clone().EnumerateObject().Single());
            }

            result.AddRange(filterProperties
                .Select(n => CreateFilter(n.Name, n.Value)));

            return result;
        }

        private ProductsOptionsExpand[] GetExpand(string name)
        {
            if (_json.TryGetProperty(name, out var expand) && expand.ValueKind == JsonValueKind.Array)
            {
                return expand.EnumerateArray()
                    .Select(elem =>
                    {
                        var expandOptions = new ProductsOptionsExpand().BuildFromJson<ProductsOptionsExpand>(elem, ElasticOptions);
                        expandOptions.Take = HighloadCommonConstants.ExpandTakeAsAll;
                        return expandOptions;
                    })
                    .ToArray();
            }
    

            return null;
        }

        protected string[] GetJsonStringArray(JsonElement value, bool skipSplit = false)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                return value.EnumerateArray()
                    .Select(n => n.ToString())
                    .ToArray();
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return skipSplit
                    ? new[] { value.GetString() }
                    : value.GetString().Split(',')
                        .Select(x => x.Trim())
                        .ToArray();   
            }
            
            return new[] { value.ToString() };
        }

        protected string[] GetJsonStringArray(string name, bool skipSplit = false)
        {
            if (_json.TryGetProperty(name, out var value))
            {
                return GetJsonStringArray(value, skipSplit);
            }
            return Array.Empty<string>();
        }
        
        protected decimal? GetJsonNullableDecimal(string name)
        {
            return _json.TryGetProperty(name, out var value) && value.TryGetDecimal(out decimal result) ? result : null;
        }
        
        protected string GetJsonString(string name)
        {
            return _json.TryGetProperty(name, out var value) ? value.ToString() : null;
        }
        
        private IElasticFilter CreateFilter(string key, JsonElement token)
        {
            var name = GetParameterName(key, out var isDisjunction);

            if (name == HighloadOperators.Or || name == HighloadOperators.And)
            {
                var childProperties = token.EnumerateObject().ToArray();
                return new GroupFilter()
                {
                    IsDisjunction = name == HighloadOperators.Or,
                    Filters = childProperties.Select(n => CreateFilter(n.Name, n.Value)).ToArray()
                };
            }

            var values = GetJsonStringArray(token, true);
            var value = values.First();
            
            if (name == HighloadParams.FreeQuery)
            {
                return new QueryFilter()
                {
                    Query = value,
                    IsDisjunction = isDisjunction
                };
            }
            var match = RangeFilterRegex.Match(value);
            if (match.Success)
            {
                return new RangeFilter
                {
                    Name = name,
                    Floor = match.Groups[1].Value,
                    Ceiling = match.Groups[2].Value,
                    IsDisjunction = isDisjunction
                };
            }

            return new SimpleFilter
            {
                Name = name,
                Values = values,
                IsDisjunction = isDisjunction,
                FromJson = true
            };
        }

        private string GetParameterName(string key, out bool isDisjunction)
        {
            isDisjunction = false;

            if (key.StartsWith(ElasticOptions.DisjunctionMark))
            {
                isDisjunction = true;
                key = key.Substring(ElasticOptions.DisjunctionMark.Length);
            }

            while (key.StartsWith(ElasticOptions.EscapeCharacter))
            {
                key = key.Substring(ElasticOptions.EscapeCharacter.Length);
            }

            return key;
        }
    }
}