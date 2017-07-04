namespace QA.ProductCatalog.Infrastructure
{
    public interface IUserProvider
    {
        int GetUserId();

        string GetUserName();
    }
}
