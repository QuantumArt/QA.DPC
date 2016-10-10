using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public interface IProductPostProcessor
    {
        JObject Process(ProductPostProcessorData product);
    }
}
