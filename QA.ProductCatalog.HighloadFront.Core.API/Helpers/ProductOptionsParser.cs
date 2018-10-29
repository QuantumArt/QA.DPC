using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public class ProductOptionsParser
    {
        private static Regex RangeFilterRegex { get; } = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");

        public static ProductsOptions Parse(IQueryCollection queryCollection, SonicElasticStoreOptions options)
        {
            if (!queryCollection.Any()) return null;

            var properties = queryCollection;

            var sort = properties["sort"].FirstOrDefault();
            var order = properties["order"].FirstOrDefault();
            var fields = properties["fields"].FirstOrDefault();
            var page = properties["page"].FirstOrDefault();
            var perPage = properties["per_page"].FirstOrDefault();
            var disableOr = properties["disable_or"].FirstOrDefault();
            var disableNot = properties["disable_not"].FirstOrDefault();
            var disableLike = properties["disable_like"].FirstOrDefault();

            var exceptKeys = new[] {"sort", "order", "fields", "page", "per_page", "disable_or", "disable_not", "disable_like", "customerCode" };
            var filters = queryCollection.Where(n => !exceptKeys.Contains(n.Key)).ToArray();

            var result = new ProductsOptions
            {
                Sort = sort,
                OrderDirection = order,
                PropertiesFilter = fields?.Split(',').ToList(),
                DisableOr = string.IsNullOrEmpty(disableOr) ? new string[] { } : disableOr.Split(','),
                DisableNot = string.IsNullOrEmpty(disableNot) ? new string[] { } : disableNot.Split(','),
                DisableLike = string.IsNullOrEmpty(disableLike) ? new string[] { } : disableLike.Split(','),
                Filters = filters.Select(n => CreateFilter(n, options)).ToList()
            };

            int intPage;
            if (int.TryParse(page, out intPage))
            {
                result.Page = intPage;
            }

            int intPerPage;
            if (int.TryParse(perPage, out intPerPage))
            {
                result.PerPage = intPerPage;
            }

            return result;
        }

        public static IElasticFilter CreateFilter(KeyValuePair<string, StringValues> pair, SonicElasticStoreOptions options)
        {
            bool isDisjunction;
            var name = GetParameterName(pair.Key, options, out isDisjunction);
            if (name == "q")
            {
                return new QueryFilter()
                {
                    Query = pair.Value,
                    IsDisjunction = isDisjunction
                };
            }

            var match = RangeFilterRegex.Match(pair.Value);
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
                Values = pair.Value,
                IsDisjunction = isDisjunction
            };

        }

        private static string GetParameterName(string key, SonicElasticStoreOptions options, out bool isDisjunction)
        {
            isDisjunction = false;

            if (key.StartsWith(options.DisjunctionMark))
            {
                isDisjunction = true;
                key = key.Substring(options.DisjunctionMark.Length);
            }

            while (key.StartsWith(options.EscapeCharacter))
            {
                key = key.Substring(options.EscapeCharacter.Length);
            }

            return key;
        }
    }
}