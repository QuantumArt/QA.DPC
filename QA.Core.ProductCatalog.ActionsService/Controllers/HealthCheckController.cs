using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.QP.Services;
using System.Linq;

namespace QA.Core.ProductCatalog.ActionsService.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class HealthCheckController: ControllerBase
    {
        private readonly IFactory _factory;
        private readonly ActionsService _servive;

        public HealthCheckController(IFactory factory, ActionsService service)
        {
            _factory = factory;
            _servive = service;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var codes = _factory.CustomerMap.Keys.ToArray();
            return Ok(new {
                CustomerCodes = codes
            });
        }

        [HttpGet]
        [Route("{customerCode}")]
        public ActionResult GetCustomerCode(string customerCode)
        {
            var state = _factory.GetState(customerCode).ToString();
            var runners = new string[0];
            bool schedulerIsRunning = false;

            var context = _servive.GetContext(customerCode);

            if (context != null)
            {
                schedulerIsRunning = context.SchedulerRunner.IsRunning;
                runners = context.Runners.Select(x => x.State.ToString()).ToArray();
            }      

            return Ok(new
            {
                State = state,
                Runners = runners,
                SchedulerIsRunning = schedulerIsRunning,
                CustomerCode = customerCode
            });
        }
    }
}
