using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mn")]
    public class InternationalCallsController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        public InternationalCallsController(ISearchRepository searchRepo)
        {
            _searchRepo = searchRepo;
        }
    }
}
