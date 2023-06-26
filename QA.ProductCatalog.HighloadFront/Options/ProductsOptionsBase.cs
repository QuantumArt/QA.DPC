using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
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

        private object _json;
        private JObject _jObj;

        protected JObject Jobj => _jObj;
        
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
        private IList<SimpleFilter> SimpleFilters => Filters.OfType<SimpleFilter>().ToList();

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

        public T BuildFromJson<T>(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
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
            return _json == null
                ? null
                : $"Id: {Id}, Skip: {Skip}, Take: {Take} {_json}";
        }

        protected virtual ProductsOptionsBase BuildFromJson(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
        {
            _json = json;
            ElasticOptions = options ?? new SonicElasticStoreOptions();

            if (json is not JObject jObj)
            {
                return this;
            }

            _jObj = jObj;

            Id = id ?? 0;
            Page = (decimal?)_jObj.SelectToken(HighloadParams.Page);
            PerPage = (decimal?)_jObj.SelectToken(HighloadParams.PerPage);
            Skip = skip ?? (decimal?)_jObj.SelectToken(HighloadParams.Skip);
            Take = take ?? (decimal?)_jObj.SelectToken(HighloadParams.Take);
            CacheForSeconds = (decimal?)_jObj.SelectToken(HighloadParams.CacheForSeconds) ?? 0;

            PropertiesFilter = JTokenToStringArray(_jObj.SelectToken(HighloadParams.Fields));
            DisableOr = JTokenToStringArray(_jObj.SelectToken(HighloadParams.DisableOr));
            DisableNot = JTokenToStringArray(_jObj.SelectToken(HighloadParams.DisableNot));
            DisableLike = JTokenToStringArray(_jObj.SelectToken(HighloadParams.DisableLike));

            Sort = (string)_jObj.SelectToken(HighloadParams.Sort);
            OrderDirection = (string)_jObj.SelectToken(HighloadParams.Order);
            Filters = GetFilters(_jObj, Id);
            DataFilters = GetDataFilters(_jObj);
            Expand = GetExpand(_jObj.SelectToken(HighloadParams.Expand));

            return this;
        }

        private Dictionary<string, string> GetDataFilters(JObject jobj)
        {
            var result = new Dictionary<string, string>();
            var dfToken = jobj.SelectToken(HighloadParams.DataFilters);
            if (dfToken != null)
            {
                return dfToken.Children().OfType<JProperty>()
                    .ToDictionary(n => n.Name, n => n.Value.ToString());
            }
            return result;
        }

        private IList<IElasticFilter> GetFilters(JObject jobj, int id)
        {
            var result = new List<IElasticFilter>();
            if (id != 0)
            {
                result.Add(CreateFilter("Id", id));
            }            
            
            var queryToken = jobj.SelectToken(HighloadParams.Query);
            JProperty[] filterProperties;
            if (queryToken != null)
            {
                filterProperties = queryToken.Children().OfType<JProperty>().ToArray();
            }
            else
            {
                var props = jobj.Children().OfType<JProperty>();
                filterProperties = props
                    .Where(n => !FirstLevelReservedKeywords.Contains(n.Name))
                    .ToArray();
            }

            result.AddRange(filterProperties
                .Select(n => CreateFilter(n.Name, n.Value)));

            return result;
        }

        private ProductsOptionsExpand[] GetExpand(JToken valuesToken)
        {
            if (valuesToken is JArray array)
            {
                return array
                    .Select(jobj =>
                    {
                        var expandOptions = new ProductsOptionsExpand().BuildFromJson<ProductsOptionsExpand>(jobj, ElasticOptions);
                        expandOptions.Take = HighloadCommonConstants.ExpandTakeAsAll;
                        return expandOptions;
                    })
                    .ToArray();
            }

            return null;
        }

        private string[] JTokenToStringArray(JToken valuesToken, bool skipSplit = false)
        {
            if (valuesToken == null)
            {
                return Array.Empty<string>();
            }
            
            if (valuesToken is JArray array)
            {
                return array
                    .Select(x => x.Value<string>())
                    .ToArray();
            }

            var values = (string) valuesToken;

            return skipSplit
                ? new[] { values }
                : values.Split(',')
                    .Select(x => x.Trim())
                    .ToArray();
        }
        
        private IElasticFilter CreateFilter(string key, JToken token)
        {
            var name = GetParameterName(key, out var isDisjunction);

            if (name == HighloadOperators.Or || name == HighloadOperators.And)
            {
                var childProperties = token.Children().OfType<JProperty>().ToArray();
                return new GroupFilter()
                {
                    IsDisjunction = name == HighloadOperators.Or,
                    Filters = childProperties.Select(n => CreateFilter(n.Name, n.Value)).ToArray()
                };
            }

            var values = JTokenToStringArray(token, true);
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