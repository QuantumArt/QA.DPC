using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductWritePostProcessor
    {
        JObject Process(ProductPostProcessorData product);
    }
}
