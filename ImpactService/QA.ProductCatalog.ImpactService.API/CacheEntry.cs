using System;
using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.ImpactService.API
{
    public class CacheEntry
    {
        public ActionResult Product { get; set; }

        public int[] Ids { get; set; }

        public DateTimeOffset LastModified { get; set; }

    }
}