using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductPostProcessor
    {
        JObject Process(ProductPostProcessorData product);
    }
}
