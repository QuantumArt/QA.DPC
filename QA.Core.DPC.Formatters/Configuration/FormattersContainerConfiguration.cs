using System;
using System.Net.Http.Formatting;
using Microsoft.Practices.Unity;
using QA.Core.DocumentGenerator;
using QA.Core.DPC.Formatters.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Configuration
{
	public class FormattersContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<PdfProductFormatter>(new InjectionFactory(c => new PdfProductFormatter(c.Resolve<IDocumentGenerator>(), c.Resolve<XmlProductFormatter>())));
			Container.RegisterType<Func<string, MediaTypeFormatter>>(new InjectionFactory(c => (Func<string, MediaTypeFormatter>)(name => c.Resolve<MediaTypeFormatter>(name))));
			Container.RegisterType<Func<Type, MediaTypeFormatter>>(new InjectionFactory(c => (Func<Type, MediaTypeFormatter>)(type => c.Resolve<MediaTypeFormatter>(type.Name))));

			Container.RegisterType<Func<string, IArticleFormatter>>(new InjectionFactory(c => new Func<string, IArticleFormatter>(name => c.Resolve<IArticleFormatter>(name))));

			var a = typeof(FormattersContainerConfiguration).Assembly;
			var i = typeof(IArticleFormatter);

			foreach (var t in a.GetExportedTypes())
			{
				if (i.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType(i, t, t.Name);
				}
			}
		}
	}
}
