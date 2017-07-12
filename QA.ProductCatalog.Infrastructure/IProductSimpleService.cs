namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductSimpleService<TProduct, TDefinition>
        where TProduct : class
        where TDefinition : class
    {
        TProduct GetProduct(string customerCode, int productId, int definitionId, bool isLive = false);
        TDefinition GetDefinition(string customerCode, int definitionId);
    }
}