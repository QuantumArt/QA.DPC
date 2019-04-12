using System;
using System.Net.Http;
using QA.ProductCatalog.Integration.Notifications;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly IHttpClientFactory _factory;
        private readonly string _restUrl;
        private readonly string _wcfUrl;
       

        public NotificationController(IIdentityProvider identityProvider, IOptions<IntegrationProperties> props, IHttpClientFactory factory)
		{
            _identityProvider = identityProvider;            
            _factory = factory;
            _restUrl = props.Value.RestNotificationUrl;
            _wcfUrl = props.Value.WcfNotificationUrl;

            if (String.IsNullOrEmpty(_restUrl) && !String.IsNullOrEmpty(_wcfUrl))
            {
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress(_wcfUrl);            
                _service = new NotificationServiceClient(myBinding, myEndpoint);              
            }
        }
        
		public ActionResult Index()
		{
            try
            {
                var customerCode = _identityProvider.Identity.CustomerCode;
                object model;

                if (!String.IsNullOrEmpty(_restUrl))
                {
                    var client = _factory.CreateClient();
                    var result = client.GetAsync(GetUrl(customerCode)).Result.Content.ReadAsStringAsync().Result;
                    model = JsonConvert.DeserializeObject<ConfigurationInfo>(result);

                }
                else
                {
                    model = _service.GetConfigurationInfo(customerCode);                    
                }

                return View(model);
            }
            catch(EndpointNotFoundException)
            {
                return View((object)null);
            }
		}

        private string GetUrl(string customerCode)
        {
            return _restUrl + "/notification/config?customerCode=" + customerCode;
        }

        public ActionResult UpdateConfiguration()
		{
            var customerCode = _identityProvider.Identity.CustomerCode;
            if (!String.IsNullOrEmpty(_restUrl))
            {
                var client = _factory.CreateClient();            
                client.PostAsync(GetUrl(customerCode), null);

            }
            else
            {
                _service.UpdateConfiguration(customerCode);
            }            
            
			return RedirectToAction("Index");
		}
	}
}