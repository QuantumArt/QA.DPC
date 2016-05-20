using System.Threading;
using System.Threading.Tasks;
using QA.Scheduler.API.Services;
using QA.ProductCatalog.RegionSync.Core.Services;

namespace QA.ProductCatalog.RegionSync.Core.Processors
{
	public class RegionSyncProcessor : IProcessor
	{
		private readonly IRegionService _regionService;
		
		public RegionSyncProcessor(IRegionService regionService)
		{
			_regionService = regionService;
		}

		public async Task Run(CancellationToken token)
		{
			await Task.Yield();
			_regionService.Run(token);
		}
	}
}
