using QA.Core.DPC.QP.Services;
using System.Linq;
using System.Web.Http;

namespace QA.ProductCatalog.WebApi.Controllers
{
    [RoutePrefix("statistic")]
    public class StatisticController : ApiController
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
        [Route("Cache/{format:media_type_mapping=json}")]        
        public string[] Cache()
        {
            return _factory.Invalidator.Keys.ToArray();
        }
    }
}