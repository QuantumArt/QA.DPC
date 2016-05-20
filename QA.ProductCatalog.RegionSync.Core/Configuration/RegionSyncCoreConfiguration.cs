using Microsoft.Practices.Unity;
using QA.Core;
using QA.ProductCatalog.RegionSync.Core.Services;
using QA.ProductCatalog.RegionSync.Core.Exstensions;

namespace QA.ProductCatalog.RegionSync.Core.Configuration
{
	public class RegionSyncCoreConfiguration : UnityContainerExtension
	{
		public const string SourceKey = "Source";
		public const string TargetKey = "Target";

		protected override void Initialize()
		{
			//Container.RegisterRegionProviderConfiguration(SourceKey);
			//Container.RegisterRegionProviderConfiguration(TargetKey);
		}
	}
}
