using System.Collections.Generic;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
    public interface IProductChangeSubscriber
	{
		void NotifyProductsChanged(Dictionary<int, int[]> affectedProductIdsByContentId);
	}
}
