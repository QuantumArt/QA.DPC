using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptions
    {
        public ProductsOptions() : this(null, null)
        {
        }

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
        
        private const string FREE_QUERY = "q";
        private const string OR_QUERY = "or";
        private const string AND_QUERY = "and";
        

        public HashSet<string> ReservedKeywords;
        
        public ProductsOptions(object json, SonicElasticStoreOptions options)
        {
            _elasticOptions = options;
            ReservedKeywords = new HashSet<string>()
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
                QUERY
            };
            
            Filters = new IElasticFilter[] { };
            if (!(json is JObject jobj)) return;
            
            Page = (decimal?) jobj.SelectToken(PAGE);
            PerPage = (decimal?) jobj.SelectToken(PER_PAGE);
            Skip = (decimal?) jobj.SelectToken(SKIP);
            Take = (decimal?) jobj.SelectToken(TAKE);
            PropertiesFilter = JTokenToStringArray(jobj.SelectToken(FIELDS));
            
            DisableOr = JTokenToStringArray(jobj.SelectToken(DISABLE_OR));
            DisableNot = JTokenToStringArray(jobj.SelectToken(DISABLE_NOT));
            DisableLike = JTokenToStringArray(jobj.SelectToken(DISABLE_LIKE));
            
            Sort = (string) jobj.SelectToken(SORT);

            var queryToken = jobj.SelectToken(QUERY);
            IEnumerable<JProperty> filterProperties;
            if (queryToken != null)
            {
                filterProperties = queryToken.Children().OfType<JProperty>().ToArray();
            }
            else
            {
                var props = jobj.Children().OfType<JProperty>();
                filterProperties = props
                    .Where(n => !ReservedKeywords.Contains(n.Name))
                    .ToArray();              
            }
            
            Filters = filterProperties
                .Select(n => CreateFilter(n.Name, n.Value))
                .ToArray();
        }

        private string[] JTokenToStringArray(JToken fields)
        {
            if (fields == null) return new string[] {};
            
            if (fields is JArray array)
            {
                return array.Select(n => n.Value<string>()).ToArray();
            }

            return ((string) fields).Split(',').Select(n => n.Trim()).ToArray();
        }

        [ModelBinder(Name = "type")]
        public string Type { get; set; }
        
        [ModelBinder(Name = "id")]
        public int Id { get; set; }
        
        [ModelBinder(Name = FIELDS)]
        public IList<string> PropertiesFilter { get; set; }

        public IList<IElasticFilter> Filters { get; set; }

        [BindNever]
        public IList<SimpleFilter> SimpleFilters => Filters.OfType<SimpleFilter>().ToList();

        [BindNever]
        public IList<RangeFilter> RangeFilters => Filters.OfType<RangeFilter>().ToList();

        [BindNever]
        public string Query => Filters.OfType<QueryFilter>().FirstOrDefault()?.Query;

        [BindNever] public bool DirectOrder => OrderDirection == "asc";
        
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

        private SonicElasticStoreOptions _elasticOptions;


        public IElasticFilter CreateFilter(string key, JToken token)
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

            var values = JTokenToStringArray(token);
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
                IsDisjunction = isDisjunction
            };

        }

        private string GetParameterName(string key, out bool isDisjunction)
        {
            isDisjunction = false;

            if (key.StartsWith(_elasticOptions.DisjunctionMark))
            {
                isDisjunction = true;
                key = key.Substring(_elasticOptions.DisjunctionMark.Length);
            }

            while (key.StartsWith(_elasticOptions.EscapeCharacter))
            {
                key = key.Substring(_elasticOptions.EscapeCharacter.Length);
            }

            return key;
        }
    }
};