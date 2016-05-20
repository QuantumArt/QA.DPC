namespace QA.ProductCatalog.Infrastructure
{
    public interface IDBConnector
    {
        string GetUrlForFileAttribute(int fieldId);
    }
}
