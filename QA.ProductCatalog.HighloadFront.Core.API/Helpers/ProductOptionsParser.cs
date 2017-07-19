using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Core.API.Helpers
{
    public class ProductOptionsParser
    {
        private static Regex RangeFilterRegex { get; } = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");
        private static string EscapeCharacter { get; } = "@";

        public static ProductsOptions Parse(IQueryCollection queryCollection)
        {
            if (!queryCollection.Any()) return null;

            var properties = queryCollection;

            var sort = properties["sort"].FirstOrDefault();
            var order = properties["order"].FirstOrDefault();
            var fields = properties["fields"].FirstOrDefault();
            var page = properties["page"].FirstOrDefault();
            var perPage = properties["per_page"].FirstOrDefault();
            var query = properties["q"].FirstOrDefault();
            var disableOr = properties["disable_or"].FirstOrDefault();
            var disableNot = properties["disable_not"].FirstOrDefault();

            var exceptKeys = new[] {"sort", "order", "fields", "page", "per_page", "q", "disable_or", "disable_not", "customerCode"};

            var filters = queryCollection.Where(n => !exceptKeys.Contains(n.Key)).ToArray();
            var rangeFilters = filters.Where(f => RangeFilterRegex.IsMatch(f.Value)).ToArray();
            var simpleFilters = filters.Except(rangeFilters);

            var result = new ProductsOptions
            {
                Sort = sort,
                Order = order == "asc",
                PropertiesFilter = fields?.Split(',').ToList(),
                DisableOr = string.IsNullOrEmpty(disableOr) ? new string[] { } : disableOr.Split(','),
                DisableNot = string.IsNullOrEmpty(disableNot) ? new string[] { } : disableNot.Split(',')
            };

            if (filters.Any())
            {
                result.Filters = simpleFilters
                    .Select(f => new SimpleFilter
                    {
                        Name = GetParameterName(f.Key),
                        Values = f.Value
                    }).ToList();
            }

            if (rangeFilters.Any())
            {
                result.RangeFilters = rangeFilters.Select(f =>
                {
                    var match = RangeFilterRegex.Match(f.Value);
                    return new RangeFilter
                    {
                        Name = GetParameterName(f.Key),
                        Floor = match.Groups[1].Value,
                        Ceiling = match.Groups[2].Value
                    };
                }).ToList();
            }

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

            result.Query = query;

            return result;
        }

        private static string GetParameterName(string key)
        {
            return key.StartsWith(EscapeCharacter) ? key.Substring(1) : key;
        }
    }
}