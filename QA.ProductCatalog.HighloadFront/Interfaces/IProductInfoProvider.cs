using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Interfaces;

public interface IProductInfoProvider
{
    string GetId(JObject product, string path);

    string GetType(JObject product, string path, string defaultType);
}
