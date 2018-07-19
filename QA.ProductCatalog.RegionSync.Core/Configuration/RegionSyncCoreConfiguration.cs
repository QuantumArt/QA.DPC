using Unity.Extension;

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
