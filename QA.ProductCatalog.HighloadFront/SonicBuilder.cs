using System;
using System.Reflection;
using Autofac;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront
{
    public class SonicBuilder
    {
        public SonicBuilder(ContainerBuilder builder)
        {
            Builder = builder;
        }


        public ContainerBuilder Builder { get; private set; }

        private SonicBuilder RegisterScoped(Type serviceType, Type concreteType)
        {
            Builder.RegisterScoped(serviceType, concreteType);
            return this;
        }

 public virtual SonicBuilder AddErrorDescriber<TDescriber>() where TDescriber : SonicErrorDescriber
        {
            return RegisterScoped(typeof(SonicErrorDescriber), typeof(TDescriber));
        }

        public virtual SonicBuilder AddProductStore<T>() where T : class
        {
            Builder.RegisterSingleton(typeof(IProductStore), typeof(T));
            return this;
        }

        public virtual SonicBuilder AddProductManager<TProductManager>() where TProductManager : class
        {
            var productManagerType = typeof(ProductManager);
            var customType = typeof(TProductManager);
            if (productManagerType == customType ||
                !productManagerType.GetTypeInfo().IsAssignableFrom(customType.GetTypeInfo()))
            {
                throw new InvalidOperationException($"Type {customType.Name} must derive from ProductManager.");
            }
            Builder.RegisterScoped(customType, services => services.Resolve(productManagerType));
            return RegisterScoped(productManagerType, customType);
        }
    }
}