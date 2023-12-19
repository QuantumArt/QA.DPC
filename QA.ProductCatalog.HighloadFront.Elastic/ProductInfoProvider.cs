using System.Text.Json;
using System.Text.Json.Nodes;
using QA.ProductCatalog.HighloadFront.Interfaces;

namespace QA.ProductCatalog.HighloadFront.Elastic;

public class ProductInfoProvider : IProductInfoProvider
{
    public string GetId(JsonElement product, string path)
    {
        var found = product.TryGetProperty(path, out var idResult);
        return found ? idResult.ToString() : null;
    }
    
    public string GetType(JsonElement product, string path, string defaultType)
    {
        var found = product.TryGetProperty(path, out var typeResult);
        return found ? typeResult.ToString() : defaultType;
    }

    public string GetId(JsonObject product, string path)
    {
        return product[path].ToString();
    }

    public string GetType(JsonObject product, string path, string defaultType)
    {
        return product[path]?.ToString() ?? defaultType;
    }
}
