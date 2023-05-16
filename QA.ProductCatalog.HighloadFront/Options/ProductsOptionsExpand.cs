// ReSharper disable InconsistentNaming

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptionsExpand : ProductsOptionsBase
    { 
        public ProductsOptionsExpand(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
            : base(json, options, id, skip, take)
        {
            if (!(json is JObject jobj))
            {
                return;
            }

            Name = (string)jobj.SelectToken(NAME);
            Path = (string)jobj.SelectToken(PATH);
        }

        [ModelBinder(Name = NAME)]
        public string Name { get; set; }

        [ModelBinder(Name = PATH)]
        public string Path { get; set; }

        public void FilterByIds(int[] ids)
        {
            var idsFilter = new SimpleFilter { Name = "Id", Values = new[] { string.Join(",", ids) } };
            Filters.Add(idsFilter);
        }
    }
}