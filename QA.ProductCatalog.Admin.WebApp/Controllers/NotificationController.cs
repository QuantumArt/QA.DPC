using QA.Core.Web;
using QA.ProductCatalog.Integration.Notifications;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
	[RequireCustomAction]
	public class NotificationController : Controller
	{
		private NotificationServiceClient _service;

		public NotificationController()
		{
			_service = new NotificationServiceClient();
		}
		public ActionResult Index()
		{
			object model = _service.GetConfigurationInfo();

			return View(model);
		}

		public ActionResult UpdateConfiguration()
		{
			_service.UpdateConfiguration();
			return RedirectToAction("Index");
		}
	}
}