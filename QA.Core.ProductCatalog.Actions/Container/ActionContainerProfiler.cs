using QA.Core.DPC.Loader;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Decorators;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.Core.DPC.Loader.Services;
using NLog;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.Core.ProductCatalog.Actions.Container
{
    public class ActionContainerProfiler : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterFactory<IArticleService>(c => new ArticleServiceProfiler(c.Resolve<ArticleServiceAdapter>()));
			Container.RegisterFactory<IProductService>(c => new ProductServiceProfiler(c.Resolve<ProductLoader>()));
			Container.RegisterFactory<IQPNotificationService>(c => new QPNotificationServiceProfiler(c.Resolve<QPNotificationService>()));
			Container.RegisterFactory<IXmlProductService>(c => new XmlProductServiceProfiler(c.Resolve<XmlProductService>()));

			var a = typeof(ActionContainerConfiguration).Assembly;
			foreach (var t in a.GetExportedTypes())
			{
				if (typeof(IAction).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterFactory<IAction>(t.Name, c => new ActionProfiler((IAction)c.Resolve(t)));
				}
			}
		}
	}
}
