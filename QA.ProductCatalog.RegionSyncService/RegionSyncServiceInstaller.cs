using System.ComponentModel;
using QA.Scheduler.Core.Service;

namespace QA.ProductCatalog.RegionSync.Service
{
	[RunInstaller(true)]
	public partial class RegionSyncServiceInstaller : ServiceInstaller<RegionSyncServiceConfiguration>
	{	
	}
}
