using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductOptionsParser
    {
        private static Regex RangeFilterRegex { get; } = new Regex(@"\[([^&=,\[\]]*),([^&=,\[\]]*)\]");

        public static IElasticFilter CreateFilter(KeyValuePair<string, StringValues> pair, SonicElasticStoreOptions options)
        {
            var name = GetParameterName(pair.Key, options, out var isDisjunction);
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