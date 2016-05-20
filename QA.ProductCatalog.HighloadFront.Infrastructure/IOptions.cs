namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public interface IOptions<out TOptions> where TOptions : class
    {
        TOptions Value { get; }
    }
}