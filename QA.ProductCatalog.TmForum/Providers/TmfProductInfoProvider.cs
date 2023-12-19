using System.Text.Json;
using System.Text.Json.Nodes;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Providers;

public class TmfProductInfoProvider : IProductInfoProvider
{
    public string GetId(JsonElement product, string path)
    {
        var found = product.TryGetProperty(InternalTmfSettings.InternalIdFieldName, out var idResult);
        return found ? idResult.ToString() : null;
    }

    public string GetType(JsonElement product, string path, string defaultType)
    {
        var found = product.TryGetProperty(InternalTmfSettings.InternalTypeFieldName, out var idResult);
        return found ? idResult.ToString() : defaultType;
    }

    public string GetId(JsonObject product, string path)
    {
        return product[InternalTmfSettings.InternalIdFieldName].ToString();
    }

    public string GetType(JsonObject product, string path, string defaultType)
    {
        return product[InternalTmfSettings.InternalTypeFieldName].ToString();
    }
}
