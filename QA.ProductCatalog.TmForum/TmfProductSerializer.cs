using Newtonsoft.Json.Linq;
using QA.Core.DPC.Front;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum;

public class TmfProductSerializer : IProductSerializer
{
    public ProductInfo Deserialize(string data)
    {
        JToken json = JObject.Parse(data)["product"];

        if (json is null)
        {
            throw new FormatException($"Unable to find product in received data: {data}");
        }

        if (!int.TryParse(json[InternalTmfSettings.InternalIdFieldName]?.ToString(), out int productId))
        {
            throw new FormatException($"Unable to find product Id in document: {json}");
        }

        string productType = json[InternalTmfSettings.InternalTypeFieldName]?.ToString();

        if (string.IsNullOrWhiteSpace(productType))
        {
            throw new FormatException($"Unable to find product type in document: {json}");
        }

        string productName = json["name"]?.ToString();

        Product product = new()
        {
            Id = productId,
            Title = productName,
            ProductType = productType,
            Alias = productName,
            MarketingProduct = new(),
            Regions = Array.Empty<Region>()
        };

        return new()
        {
            Products = new[] { product }
        };
    }
}
