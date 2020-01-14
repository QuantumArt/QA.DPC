using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tasks
{
	public class EmptyTaskExecutionContext : ITaskExecutionContext
	{
		public EmptyTaskExecutionContext()
		{
			IsCancellationRequested = false;
			IsCancelled = false;
		}

		#region ITaskExecutionContext implementation
		public void SetProgress(byte progress)
		{
		}

		public ActionTaskResult Result { get; set; }		
		public bool IsCancellationRequested { get; set; }
		public bool IsCancelled { get; set; }
		#endregion
	}
}
