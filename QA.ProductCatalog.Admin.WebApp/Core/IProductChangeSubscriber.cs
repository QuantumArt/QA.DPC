using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
	public interface IProductChangeSubscriber
	{
		void NotifyProductsChanged(Dictionary<int, int[]> affectedProductIdsByContentId);
	}
}
