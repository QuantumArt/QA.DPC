using Autofac;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront
{
    public static class SonicContainerBuilderExtensions
    {
        public static SonicBuilder RegisterSonic(this ContainerBuilder builder)
        {
            // Builder used by Sonic
            builder.RegisterOptions();
            builder.RegisterScoped<IProductValidator, ProductValidator>();

            builder.RegisterSingleton<ProductManager>();


            builder.RegisterScoped<SonicErrorDescriber>();

            return new SonicBuilder(builder);
        }
    }
}