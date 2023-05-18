using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptionsExpand : ProductsOptionsBase
    {
        protected override ProductsOptionsBase BuildFromJson(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
        {
            base.BuildFromJson(json, options, id, skip, take);

            if (Jobj == null)
            {
                return this;
            }

            Name = (string)Jobj.SelectToken(NAME);
            Path = (string)Jobj.SelectToken(PATH);

            return this;
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