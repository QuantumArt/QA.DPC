namespace QA.ProductCatalog.Infrastructure
{
    public interface IStatusProvider
    {
        string GetStatusName(int statusId);
    }
}
