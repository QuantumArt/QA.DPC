namespace QA.ProductCatalog.RegionSync.Core.Services
{
	public interface IRegionProviderConfiguration
	{
		string ConnectionString { get; }
		int RegionContentId { get; }
		int UserId { get; }
	}
}
