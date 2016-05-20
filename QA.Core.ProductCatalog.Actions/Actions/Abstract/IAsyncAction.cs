using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public interface IAsyncAction
	{
		Task<string> Process(ActionContext context);
	}
}