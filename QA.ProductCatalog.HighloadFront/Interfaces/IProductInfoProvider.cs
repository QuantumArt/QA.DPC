using System.Text.Json;
using System.Text.Json.Nodes;

namespace QA.ProductCatalog.HighloadFront.Interfaces;

public interface IProductInfoProvider
{
    string GetId(JsonElement product, string path);

    string GetType(JsonElement product, string path, string defaultType);
    
    string GetId(JsonObject product, string path);

    string GetType(JsonObject product, string path, string defaultType);
}
