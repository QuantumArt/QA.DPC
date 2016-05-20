using System.Threading;

namespace QA.ProductCatalog.RegionSync.Core.Services
{
	public interface IRegionService
	{
		void Run(CancellationToken token);
	}
}