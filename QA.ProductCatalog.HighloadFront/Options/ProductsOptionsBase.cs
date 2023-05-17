using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace QA.ProductCatalog.HighloadFront.Options
{
    public abstract class ProductsOptionsBase
    {
        private static Regex RangeFilterRegex { get; } = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");
        
        private const string PAGE = "page";
        private const string PER_PAGE = "per_page";
        private const string SKIP = "skip";
        private const string TAKE = "take";
        private const string FIELDS = "fields";
        private const string SORT = "sort";
        private const string ORDER = "order";
        private const string DISABLE_OR = "disable_or";
        private const string DISABLE_NOT = "disable_not";
        private const string DISABLE_LIKE = "disable_like";
        private const string QUERY = "query";
        private const string DATA_FILTERS = "data_filters";
        private const string CACHE_FOR_SECONDS = "cache_for_seconds";
        private const string EXPAND = "expand";
        protected const string PATH = "path";
        protected const string NAME = "name";

        private const string FREE_QUERY = "q";
        private const string OR_QUERY = "or";
        private const string AND_QUERY = "and";

        public SonicElasticStoreOptions ElasticOptions { get; set; }
        protected object _json;

        private static readonly HashSet<string> FirstLevelReservedKeywords = new HashSet<string>()
        {
            PAGE,
            PER_PAGE,
            SKIP,
            TAKE,
            FIELDS,
            SORT,
            ORDER,
            DISABLE_OR,
            DISABLE_NOT,
            DISABLE_LIKE,
            QUERY,
            DATA_FILTERS,
            CACHE_FOR_SECONDS,
            EXPAND,
            PATH,
            NAME
        };
        
        protected ProductsOptionsBase(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
        {
            _json = json;
            ElasticOptions = options ?? new SonicElasticStoreOptions();
            
            Filters = new List<IElasticFilter>();
            DataFilters = new Dictionary<string, string>();
            
            if (!(json is JObject jobj)) return;

            Id = id ?? 0;
            Page = (decimal?) jobj.SelectToken(PAGE);
            PerPage = (decimal?) jobj.SelectToken(PER_PAGE);
            Skip = skip ?? (decimal?) jobj.SelectToken(SKIP);
            Take = take ?? (decimal?) jobj.SelectToken(TAKE);
            CacheForSeconds = (decimal?) jobj.SelectToken(CACHE_FOR_SECONDS) ?? 0;
            
            PropertiesFilter = JTokenToStringArray(jobj.SelectToken(FIELDS));            
            DisableOr = JTokenToStringArray(jobj.SelectToken(DISABLE_OR));
            DisableNot = JTokenToStringArray(jobj.SelectToken(DISABLE_NOT));
            DisableLike = JTokenToStringArray(jobj.SelectToken(DISABLE_LIKE));
            
            Sort = (string) jobj.SelectToken(SORT);
            OrderDirection = (string) jobj.SelectToken(ORDER);            
            Filters = GetFilters(jobj, Id);
            DataFilters = GetDataFilters(jobj);
            Expand = GetExpand(jobj.SelectToken(EXPAND));
        }

        public void ApplyQueryCollection(IQueryCollection collection)
        {
            Filters = collection.Where(n => !FirstLevelReservedKeywords.Contains(n.Key))
                .Select(n => ProductOptionsParser.CreateFilter(n, ElasticOptions)).ToArray();
        }

        #region Bound properties

        
        [ModelBinder(Name = "type")]
        public string Type { get; set; }
        
        [ModelBinder(Name = "id")]
        public int Id { get; set; }
        
        [ModelBinder(Name = FIELDS)]
        public string Fields { get; set; }
        
        [ModelBinder(Name = PAGE)]
        public decimal? Page { get; set; }
        
        [ModelBinder(Name = PER_PAGE)]
        public decimal? PerPage { get; set; }
        
        [ModelBinder(Name = SKIP)]
        public decimal? Skip { get; set; }

        [ModelBinder(Name = TAKE)]
        public decimal? Take { get; set; }

        [ModelBinder(Name = SORT)]
        public string Sort { get; set; }

        [ModelBinder(Name = ORDER)]
        public string OrderDirection { get; set; }

        [ModelBinder(Name = DISABLE_OR)]
        public string[] DisableOr { get; set; }

        [ModelBinder(Name = DISABLE_NOT)]
        public string[] DisableNot { get; set; }

        [ModelBinder(Name = DISABLE_LIKE)]
        public string[] DisableLike { get; set; }

        [ModelBinder(Name = EXPAND)]
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
                    .Select(f => f.Value).FirstOrDefault();
            }
        }

        #endregion

        public virtual string GetKey()
        {
            return _json != null ? $"Id: {Id}, Skip: {Skip}, Take:{Take}" + _json : null;
        }

        private Dictionary<string, string> GetDataFilters(JObject jobj)
        {
            var result = new Dictionary<string, string>();
            var dfToken = jobj.SelectToken(DATA_FILTERS);
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
            
            var queryToken = jobj.SelectToken(QUERY);
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
            if (valuesToken == null)
            {
                return null;
            }

            if (valuesToken is JArray array)
            {
                return array
                    .Select(jobj => new ProductsOptionsExpand(jobj, ElasticOptions))
                    .ToArray();
            }

            return null;
        }

        private string[] JTokenToStringArray(JToken valuesToken, bool skipSplit = false)
        {
            if (valuesToken == null) return new string[] {};
            
            if (valuesToken is JArray array)
            {
                return array.Select(n => n.Value<string>()).ToArray();
            }

            var values = (string) valuesToken;

            return skipSplit ? new[] { values } : values.Split(',').Select(n => n.Trim()).ToArray();
        }
        
        private IElasticFilter CreateFilter(string key, JToken token)
        {
            var name = GetParameterName(key, out var isDisjunction);

            if (name == OR_QUERY || name == AND_QUERY)
            {
                var childProperties = token.Children().OfType<JProperty>().ToArray();
                return new GroupFilter()
                {
                    IsDisjunction = name == OR_QUERY,
                    Filters = childProperties.Select(n => CreateFilter(n.Name, n.Value)).ToArray()
                };
            }

            var values = JTokenToStringArray(token, true);
            var value = values.First();
            
            if (name == FREE_QUERY)
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