namespace QA.ProductCatalog.Infrastructure
{
    public interface ITask
    {
        void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext);
    }
}
