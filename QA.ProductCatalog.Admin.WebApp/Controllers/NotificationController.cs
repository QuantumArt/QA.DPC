using QA.Core.DPC.QP.Servives;
using QA.Core.Web;
using QA.ProductCatalog.Integration.Notifications;
using System;
using System.ServiceModel;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
	[RequireCustomAction]
	public class NotificationController : Controller
	{
		private NotificationServiceClient _service;
        private IIdentityProvider _identityProvider;

        public NotificationController(IIdentityProvider identityProvider)
		{
            _identityProvider = identityProvider;
            _service = new NotificationServiceClient();
		}
		public ActionResult Index()
		{
            try
            {
                var customerCode = _identityProvider.Identity.CustomerCode;
                object model = _service.GetConfigurationInfo(customerCode);
                return View(model);
            }
            catch(EndpointNotFoundException)
            {
                return View((object)null);
            }
		}

		public ActionResult UpdateConfiguration()
		{
            var customerCode = _identityProvider.Identity.CustomerCode;
            _service.UpdateConfiguration(customerCode);
			return RedirectToAction("Index");
		}
	}
}