namespace QA.ProductCatalog.Infrastructure
{
    public interface IStatusProvider
    {
        string GetStatusName(int contentId, int statusId);
    }
}
