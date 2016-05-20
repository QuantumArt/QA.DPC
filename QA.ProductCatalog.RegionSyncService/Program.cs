using QA.Scheduler.Core.Service;

namespace QA.ProductCatalog.RegionSync.Service
{
	class Program
	{
		static void Main(string[] args)
		{
			ServiceRunner.RunService<RegionSyncServiceConfiguration>();	
		}
	}
}