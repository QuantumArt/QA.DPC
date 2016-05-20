using System.Threading;
using System.Threading.Tasks;

namespace QA.Scheduler.API.Services
{
	public interface IProcessor
	{
		Task Run(CancellationToken token);
	}
}