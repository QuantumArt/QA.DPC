using System.Threading.Tasks;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public interface IAsyncAction
	{
		Task<ActionTaskResult> Process(ActionContext context);
	}
}