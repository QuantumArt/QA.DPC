namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStoreFactory
    {
        IProductStore GetProductStore(string language, string state);
    }
}
