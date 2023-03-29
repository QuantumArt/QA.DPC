using QA.Core;
using QA.Core.DPC.API.Container;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.API.Container;
using QA.Core.DPC.QP.Autopublish.Configuration;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.Configuration;
using System;
using QA.Core.DocumentGenerator;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Cache;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using Unity;
using QA.ProductCatalog.Filters;
using QA.ProductCatalog.TmForum.Extensions;
using QA.Core.DPC.Workflow.Extensions;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public static class UnityConfig
    {
	    

		public static IUnityContainer Configure(IUnityContainer unityContainer, LoaderProperties loaderProps, Properties props)
		{
		    var container = RegisterTypes(unityContainer, loaderProps, props);
		    ObjectFactoryConfigurator.DefaultContainer = container;
		    return container;
        }

		private static IUnityContainer RegisterTypes(IUnityContainer unityContainer, LoaderProperties loaderProps, Properties props)
		{
			if (props == null)
				throw new ArgumentNullException(nameof(props));
			
			unityContainer.RegisterType<DynamicResourceDictionaryContainer>();
			unityContainer.RegisterType<ProcessRemoteValidationIf>();
			
			unityContainer.AddNewExtension<LoaderConfigurationExtension>();
			unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
            unityContainer.AddNewExtension<FormattersContainerConfiguration>();
			unityContainer.AddNewExtension<APIContainerConfiguration>();
            unityContainer.AddNewExtension<QPAPIContainerConfiguration>();

            //Register TMForum extension
            unityContainer.RegisterTmForum();

			//Workflow
			unityContainer.RegisterWorkflow();

            unityContainer.RegisterType<IConnectionProvider, CoreConnectionProvider>();
            unityContainer.RegisterType<ICustomerProvider, CustomerProvider>();
            unityContainer.RegisterType<IIdentityProvider, CoreIdentityProvider>();
			
			if (props.UseAuthorization)
			{
				unityContainer.RegisterType<IUserProvider, IdentityUserProvider>();
			}
			else
			{
				unityContainer.RegisterType<IUserProvider, ConfigurableUserProvider>();
			}

			unityContainer.RegisterType<ISettingsService, SettingsFromContentCoreService>();

			unityContainer.RegisterType<IRegionTagReplaceService, RegionTagService>();

			unityContainer.RegisterType<IRegionService, RegionService>();

			unityContainer.RegisterType<IXmlProductService, XmlProductService>();

			unityContainer.RegisterType<IContentDefinitionService, ContentDefinitionService>();
			
			unityContainer.RegisterType<INotesService, NotesFromContentService>();
			
			unityContainer.RegisterType<IDocumentGenerator, DocumentGenerator>();

            unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();

            unityContainer.RegisterType<StructureCacheTracker>();
            
            unityContainer.RegisterType<IWarmUpProvider, ProductLoaderWarmUpProvider>();
            
            unityContainer.RegisterSingleton<WarmUpRepeater>();
            
            switch (loaderProps.SettingsSource)
            {
	            case SettingsSource.Content:
		            unityContainer.RegisterType<ISettingsService, SettingsFromContentCoreService>();
		            break;
	            case SettingsSource.AppSettings:
		            unityContainer.RegisterType<ISettingsService, SettingsFromQpCoreService>();
		            break;
            }
            
            switch (loaderProps.DefaultSerializer)
            {
	            case DefaultSerializer.Json:
		            unityContainer.RegisterType<IArticleFormatter, JsonProductFormatter>();
		            break;
	            case DefaultSerializer.Xml:
		            unityContainer.RegisterType<IArticleFormatter, XmlProductFormatter>();
		            break;
            }

            var connection = unityContainer.Resolve<IConnectionProvider>();

            if (connection.QPMode)
            {
                unityContainer
	                .RegisterConsolidationCache(props.AutoRegisterConsolidationCache)
	                .As<IFactory>()
	                .With<FactoryWatcher>(props.WatcherInterval)
	                .Watch();
            }
            else
            {
	            unityContainer.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>();	            
                unityContainer
	                .RegisterConsolidationCache(props.AutoRegisterConsolidationCache, SingleCustomerCoreProvider.Key)
	                .As<IFactory>()
	                .With<FactoryWatcher>()
	                .WithCallback((sender, e) => { unityContainer.Resolve<WarmUpRepeater>().Start(); })	                
	                .Watch();
            }

            if (connection.QPMode || connection.UseQPMonitoring)
            {
                unityContainer.RegisterQpMonitoring();
            }
            else
            {
                unityContainer.RegisterNonQpMonitoring();
            }
            
            return unityContainer;
		}
	}
}