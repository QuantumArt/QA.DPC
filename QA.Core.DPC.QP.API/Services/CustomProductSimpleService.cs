using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.API.Services
{
    public class CustomProductSimpleService<TProduct, TDefinition> : IProductSimpleService<TProduct, TDefinition>
        where TProduct : class
        where TDefinition : class
    {
        private readonly TProduct _product;
        private readonly TDefinition _definition;

        public CustomProductSimpleService(TProduct product, TDefinition definition)
        {
            _product = product;
            _definition = definition;
        }

        public TDefinition GetDefinition(string customerCode, int definitionId)
        {
            return _definition;
        }

        public TProduct GetProduct(string customerCode, int productId, int definitionId, bool isLive = false)
        {
            return _product;
        }
    }
}