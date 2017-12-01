using QA.Core.DPC.QP.Services;
using System.Linq;
using System.Web.Http;

namespace QA.ProductCatalog.WebApi.Controllers
{
    public class StatisticController : ApiController
    {
        private readonly IFactory _factory;

        public StatisticController(IFactory factory)
        {
            _factory = factory;
        }

        [AcceptVerbs("GET")]
        public string[] Cache()
        {
            return _factory.Invalidator.Keys.ToArray();
        }
    }
}