using System;
using System.Net.Http.Formatting;
using QA.Core.DPC.Formatters.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Entities;
using QA.Core.Models.Configuration;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.Core.DPC.Formatters.Configuration
{
    public class FormattersContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterFactory<Func<string, MediaTypeFormatter>>(c => (Func<string, MediaTypeFormatter>)(name => c.Resolve<MediaTypeFormatter>(name)));
			Container.RegisterFactory<Func<Type, MediaTypeFormatter>>(c => (Func<Type, MediaTypeFormatter>)(type => c.Resolve<MediaTypeFormatter>(type.Name)));

			Container.RegisterFactory<Func<string, IArticleFormatter>>(c => new Func<string, IArticleFormatter>(name => c.Resolve<IArticleFormatter>(name)));

			var a = typeof(FormattersContainerConfiguration).Assembly;
			var i = typeof(IArticleFormatter);

			foreach (var t in a.GetExportedTypes())
			{
				if (i.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType(i, t, t.Name);
				}
			}
            
            Container.RegisterFactory<Func<XmlSchemaFormatter>>(c => new Func<XmlSchemaFormatter>(() => c.Resolve<XmlSchemaFormatter>()));
            Container.RegisterFactory<Func<XmlProductFormatter>>(c => new Func<XmlProductFormatter>(() => c.Resolve<XmlProductFormatter>()));
            Container.RegisterFactory<Func<XamlSchemaFormatter>>(c => new Func<XamlSchemaFormatter>(() => c.Resolve<XamlSchemaFormatter>()));
            Container.RegisterFactory<Func<XamlProductFormatter>>(c => new Func<XamlProductFormatter>(() => c.Resolve<XamlProductFormatter>()));

            Container.RegisterType<XmlProductFormatter>();
            Container.RegisterType<XamlProductFormatter>();
            
#if NET_FRAMEWORK          
            Container.RegisterFactory<Func<PdfProductFormatter>>(c => new Func<PdfProductFormatter>(() => c.Resolve<PdfProductFormatter>()));
#endif            
            Container.RegisterFactory<Func<JsonSchemaFormatter>>(c => new Func<JsonSchemaFormatter>(() => c.Resolve<JsonSchemaFormatter>()));
            Container.RegisterFactory<Func<JsonProductFormatter>>(c => new Func<JsonProductFormatter>(() => c.Resolve<JsonProductFormatter>()));
            Container.RegisterType<JsonProductFormatter>();
            Container.RegisterFactory<Func<BinaryModelFormatter<Article>>>(c => new Func<BinaryModelFormatter<Article>>(() => c.Resolve<BinaryModelFormatter<Article>>()));
            Container.RegisterFactory<Func<BinaryModelFormatter<Content>>>(c => new Func<BinaryModelFormatter<Content>>(() => c.Resolve<BinaryModelFormatter<Content>>()));
        }
	}
}
