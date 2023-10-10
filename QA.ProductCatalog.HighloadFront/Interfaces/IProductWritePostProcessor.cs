using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductWritePostProcessor
    {
        void Process(ProductPostProcessorData product);
    }
}
