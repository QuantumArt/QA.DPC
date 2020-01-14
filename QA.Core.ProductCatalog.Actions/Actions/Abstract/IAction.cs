using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public interface IAction
	{
		ActionTaskResult Process(ActionContext context);
	}
}