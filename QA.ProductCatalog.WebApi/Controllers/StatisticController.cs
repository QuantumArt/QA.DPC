using QA.Core.DPC.QP.Services;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.WebApi.Models;

namespace QA.ProductCatalog.WebApi.Controllers
{
    [Route("statistic")]
    public class StatisticController : Controller
    {
        private readonly IFactory _factory;

        public StatisticController(IFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Customer codes initialized with cache
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("cache/{format:regex(^json|xml|xaml|jsonDefinition|jsonDefinition2$)}")]        
        public CustomerCodeViewModel[] Cache()
        {
            return _factory.CustomerMap
                .Select(item => new CustomerCodeViewModel()
                {
                    CustomerCode = item.Key,
                    State = item.Value.State.ToString()
                }).ToArray();
        }
    }
}