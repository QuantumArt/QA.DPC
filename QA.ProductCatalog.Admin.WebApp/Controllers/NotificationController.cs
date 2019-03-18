using QA.ProductCatalog.Integration.Notifications;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Admin.WebApp.Filters;
using QA.ProductCatalog.Integration;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
	public class NotificationController : Controller
	{
		private NotificationServiceClient _service;
        private IIdentityProvider _identityProvider;

        public NotificationController(IIdentityProvider identityProvider, IOptions<IntegrationProperties> props)
		{
            var myBinding = new BasicHttpBinding();
            var myEndpoint = new EndpointAddress(props.Value.WcfNotificationUrl);            
            _identityProvider = identityProvider;
            _service = new NotificationServiceClient(myBinding, myEndpoint);
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