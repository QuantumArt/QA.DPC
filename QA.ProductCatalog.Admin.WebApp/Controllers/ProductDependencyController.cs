using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.Admin.WebApp.Core;
using QA.ProductCatalog.Admin.WebApp.Models;


namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ProductDependencyController : Controller
    {
		private readonly IProductChangeNotificator _productChangeNotificator;

		public ProductDependencyController(IProductChangeNotificator productChangeNotificator)
		{
			_productChangeNotificator = productChangeNotificator;
		}

		public ContentResult Index(ArticleChangedNotification articleChangedNotification)
		{
			_productChangeNotificator.NotifyProductsChanged(articleChangedNotification);

	        return Content("Ok");
        }
    }
}
