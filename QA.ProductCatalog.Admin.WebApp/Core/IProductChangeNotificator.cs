using System.Collections.Generic;
using QA.ProductCatalog.Admin.WebApp.Models;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
	public interface IProductChangeNotificator
	{
		void AddSubscribers(IEnumerable<IProductChangeSubscriber> subscribers);
		void NotifyProductsChanged(ArticleChangedNotification articleChangedNotification);
	}
}