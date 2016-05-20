namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public interface IConfigureOptions<in TOptions> where TOptions : class
    {
        void Configure(TOptions options);
    }
}