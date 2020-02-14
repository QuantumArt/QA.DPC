using System.Threading;
using System.Threading.Tasks;

namespace QA.ProductCatalog.RegionSync.Core.Processors
{
	public interface IProcessor
	{
		Task Run(CancellationToken token);
	}
}