using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API
{
    public class CacheEntry
    {
        public ActionResult Product { get; set; }

        public int[] Ids { get; set; }

        public DateTimeOffset LastModified { get; set; }

    }
}