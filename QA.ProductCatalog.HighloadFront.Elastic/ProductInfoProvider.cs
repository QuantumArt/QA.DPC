using System;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;

namespace QA.ProductCatalog.HighloadFront.Elastic;

public class ProductInfoProvider : IProductInfoProvider
{
    public string GetId(JObject product, string path)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        return product[path]?.ToString();
    }
    
    public string GetType(JObject product, string path, string defaultType)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        string type = product[path]?.ToString();
        if (type == null)
            return defaultType;
        
        return type;
    }
}
