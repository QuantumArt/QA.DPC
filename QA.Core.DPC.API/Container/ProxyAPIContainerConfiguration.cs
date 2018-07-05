using System;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Formatters.Services;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Generic;
using Unity;
using Unity.Extension;
using Unity.Lifetime;


namespace QA.Core.DPC.API.Container
{
    public class ProxyAPIContainerConfiguration : UnityContainerExtension
	{
		private const string BinaryMediaType = "application/octet-stream";

		protected override void Initialize()
		{
			Container.RegisterModelMediaTypeFormatter<BinaryModelFormatter<Article>, Article>(BinaryMediaType);
			Container.RegisterModelMediaTypeFormatter<BinaryModelFormatter<int[]>, int[]>(BinaryMediaType);
			Container.RegisterModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, object>[]>, Dictionary<string, object>[]>(BinaryMediaType);
			Container.RegisterModelMediaTypeFormatter<BinaryModelFormatter<Dictionary<string, string>>, Dictionary<string, string>>(BinaryMediaType);
			Container.RegisterModelMediaTypeFormatter<BinaryModelFormatter<Exception>, Exception>(BinaryMediaType);

			Container.RegisterType<IProductAPIService, ProductAPIProxy>(new HierarchicalLifetimeManager());
			Container.RegisterType<IProxyConfiguration, ProxyConfiguration>(new HierarchicalLifetimeManager());

			Container.AddNewExtension<FormattersContainerConfiguration>();
		}
	}
}
