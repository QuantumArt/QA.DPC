using System;
using System.Net.Http.Formatting;
using Microsoft.Practices.Unity;
using QA.Core.DocumentGenerator;
using QA.Core.DPC.Formatters.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Entities;
using QA.Core.Models.Configuration;

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
            
            Container.RegisterType<Func<XmlSchemaFormatter>>(new InjectionFactory(c => new Func<XmlSchemaFormatter>(() => c.Resolve<XmlSchemaFormatter>())));
            Container.RegisterType<Func<XmlProductFormatter>>(new InjectionFactory(c => new Func<XmlProductFormatter>(() => c.Resolve<XmlProductFormatter>())));
            Container.RegisterType<Func<XamlSchemaFormatter>>(new InjectionFactory(c => new Func<XamlSchemaFormatter>(() => c.Resolve<XamlSchemaFormatter>())));
            Container.RegisterType<Func<XamlProductFormatter>>(new InjectionFactory(c => new Func<XamlProductFormatter>(() => c.Resolve<XamlProductFormatter>())));
            Container.RegisterType<Func<PdfProductFormatter>>(new InjectionFactory(c => new Func<PdfProductFormatter>(() => c.Resolve<PdfProductFormatter>())));
            Container.RegisterType<Func<JsonSchemaFormatter>>(new InjectionFactory(c => new Func<JsonSchemaFormatter>(() => c.Resolve<JsonSchemaFormatter>())));
            Container.RegisterType<Func<JsonProductFormatter>>(new InjectionFactory(c => new Func<JsonProductFormatter>(() => c.Resolve<JsonProductFormatter>())));
            Container.RegisterType<Func<BinaryModelFormatter<Article>>>(new InjectionFactory(c => new Func<BinaryModelFormatter<Article>>(() => c.Resolve<BinaryModelFormatter<Article>>())));
            Container.RegisterType<Func<BinaryModelFormatter<Content>>>(new InjectionFactory(c => new Func<BinaryModelFormatter<Content>>(() => c.Resolve<BinaryModelFormatter<Content>>())));
        }
	}
}
