namespace QA.ProductCatalog.TmForum.Interfaces
{
    public interface IProductAddressProvider
    {
        Uri GetProductAddress(string type, string resourceId);
    }
}
