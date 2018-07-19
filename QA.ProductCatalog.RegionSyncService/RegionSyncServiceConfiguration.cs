using Unity;
using QA.ProductCatalog.RegionSync.Core.Processors;
using QA.ProductCatalog.RegionSync.Core.Configuration;
using QA.Scheduler.Core.Configuration;
using Unity.Extension;

namespace QA.ProductCatalog.RegionSync.Service
{
	public class RegionSyncServiceConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			var settings = RegionSyncServiceSettings.Default;

			Container.RegisterService(settings.RegionSyncKey, settings.RegionSyncName, settings.RegionSyncDescription);
			Container.RegisterProcessor<RegionSyncProcessor>(settings.RegionSyncKey, "RegionSyncSchedule");

			Container.AddNewExtension<RegionSyncCoreConfiguration>();
		}
	}
}
