using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Providers;

public class TmfProductInfoProvider : IProductInfoProvider
{
    public string GetId(JObject product, string path)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return product[InternalTmfSettings.InternalIdFieldName]?.ToString();
    }

    public string GetType(JObject product, string path, string defaultType)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        string type = product[InternalTmfSettings.InternalTypeFieldName]?.ToString();
        if (type == null)
            return defaultType;
        
        return type;
    }
}
