using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Constants;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptionsExpand : ProductsOptionsBase
    {
        private const decimal DefaultExpandCacheTimeInSeconds = 60;

        public ProductsOptionsExpand()
        {
            CacheForSeconds = DefaultExpandCacheTimeInSeconds;
        }

        [ModelBinder(Name = HighloadParams.Name)]
        public string Name { get; set; }

        [ModelBinder(Name = HighloadParams.Path)]
        public string Path { get; set; }

        public void FilterByIds(int[] ids)
        {
            var idsFilter = new SimpleFilter
            {
                Name = "Id",
                Values = new[] { string.Join(",", ids) }
            };
            Filters.Add(idsFilter);
        }

        protected override ProductsOptionsBase BuildFromJson(JsonElement json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
        {
            base.BuildFromJson(json, options, id, skip, take);

            Name = GetJsonString(HighloadParams.Name);
            Path = GetJsonString(HighloadParams.Path);
            CacheForSeconds = GetJsonNullableDecimal(HighloadParams.CacheForSeconds) ?? DefaultExpandCacheTimeInSeconds;

            return this;
        }

    }
}