namespace QA.ProductCatalog.ContentProviders
{
    public interface ITask
    {
        void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext);
    }
}
