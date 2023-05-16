using System.Linq;
// ReSharper disable InconsistentNaming

namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ProductsOptionsRoot : ProductsOptionsBase
    {
        public ProductsOptionsRoot() : base(null, null)
        {
        }
        
        public ProductsOptionsRoot(object json, SonicElasticStoreOptions options, int? id = null, int? skip = null, int? take = null)
            : base(json, options, id, skip, take)
        {
        }

        public void ComputeArrays()
        {
            PropertiesFilter = Fields?.Split(',').ToArray();

            DisableOr = (DisableOr == null || !DisableOr.Any())
                ? new string[] { }
                : DisableOr.SelectMany(n => n.Split(',')).ToArray();

            DisableNot = (DisableNot == null || !DisableNot.Any())
                ? new string[] { }
                : DisableNot.SelectMany(n => n.Split(',')).ToArray();

            DisableLike = (DisableLike == null || !DisableLike.Any())
                ? new string[] { }
                : DisableLike.SelectMany(n => n.Split(',')).ToArray();
        }
    }
}