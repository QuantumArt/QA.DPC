using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Controllers
{
    [Route("notification")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _service;
        private ConnectionProperties _cnnProps;
        
        
        public NotificationController(INotificationService service, IOptions<ConnectionProperties> cnnProps)
        {
            _service = service;
            _cnnProps = cnnProps.Value;
        }

        private string ActualCustomerCode(string customerCode)
        {
            return _cnnProps.QpMode && !string.IsNullOrEmpty(customerCode) ? customerCode : SingleCustomerCoreProvider.Key;
        }

        [HttpPut]
        public ActionResult PushNotifications([FromBody] NotificationItem[] notifications, bool isStage, int userId,
            string userName, string method, string customerCode)
        {
            try
            {
                _service.PushNotifications(notifications, isStage, userId, userName, method, ActualCustomerCode(customerCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("config")]
        public ActionResult UpdateConfiguration(string customerCode)
        {
            try
            {
                _service.UpdateConfiguration(ActualCustomerCode(customerCode));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("config")]
        public ActionResult GetConfigurationInfo(string customerCode)
        {
            return Json(_service.GetConfigurationInfo(ActualCustomerCode(customerCode)));
        }
    }
}