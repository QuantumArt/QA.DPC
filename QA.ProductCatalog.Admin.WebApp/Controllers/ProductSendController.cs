using System;
using QA.Core.Extensions;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.Admin.WebApp.Filters;
using QA.ProductCatalog.Admin.WebApp.Models;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class ProductSendController : Controller
    {
        static long numberOfSessions = 0;

        [HttpPost]
        public ActionResult Send(SendProductModel model)
        {
            var n = Interlocked.Read(ref numberOfSessions);
            if (n > 0)
            {
                ModelState.AddModelError("", "A product sending query in running state already exists. Please, wait for completing.");
                return View(model);
            }
            try
            {
                Interlocked.Increment(ref numberOfSessions);
                int[] ids = null;
                if (model != null && ModelState.IsValid)
                {
                    var idsList = model.Ids.SplitString(' ', ',', ';', '\n', '\r').Distinct().ToArray();
                    if (idsList.Length > 1000)
                    {
                        ModelState.AddModelError("", "Too much products. Please, specify no more than 1000");
                        return View(model);
                    }
                    try
                    {
                        ids = idsList.Select(x => int.Parse(x)).ToArray();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Non-number values specified: " + ex.Message);
                        return View(model);
                    }

                    if (ModelState.IsValid)
                    {
                        ModelState.Remove("Ids");
                        ModelState.Remove("ArticleIds");

                        var result = SendProductModel.Send(ids, 15);

                        model.Ids = string.Join("\n", result.Failed);
                        model.NeedPublishing = result.NeedPublishing;
                        model.ArticleIds = string.Join("\n", result.NeedPublishing.Select(x => x.Id).Distinct());
                        model.Removed = result.Removed;
                        model.NotFound = result.NotFound;

                        if (result.Failed.Length > 0 || result.Errors.Length > 0)
                        {
                            ModelState.AddModelError("", string.Format("Processed {0} from {1}, unprocessed: {2}",
                                ids.Length - result.Failed.Length,
                                ids.Length, result.Failed.Length));
                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Processed {0} from {1}", ids.Length - result.Failed.Length - result.NeedPublishing.Length, ids.Length));
                        }
                        if (result.Errors.Length > 0)
                        {
                            model.Message = string.Join("\n", result.Errors.Select(x => x + x.Message).Distinct());
                        }
                    }
                }

                return View(model);
            }
            finally
            {
                Interlocked.Decrement(ref numberOfSessions);
            }
        }

    }
}
