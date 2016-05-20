using System;
using QA.Core;
using QA.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.Core.Web;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ProductSendController : Controller
    {
        static long numberOfSessions = 0;

        [HttpPost]
        public ActionResult Send(SendProductModel model)
        {
            var n = Interlocked.Read(ref numberOfSessions);
            if (n > 0)
            {
                ModelState.AddModelError("", "Один запрос на отправку продуктов уже выполняется. Дождитесь окончания.");
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
                        ModelState.AddModelError("", "Слишком много продуктов. Укажите не более 1000");
                        return View(model);
                    }
                    try
                    {
                        ids = idsList.Select(x => int.Parse(x)).ToArray();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Указаны нечисловые значения. " + ex.Message);
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
                            ModelState.AddModelError("", string.Format("Обработано {0} из {1}, в поле указаны необработанные продукты ({2})",
                                ids.Length - result.Failed.Length,
                                ids.Length, result.Failed.Length));
                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Обработано {0} из {1}", ids.Length - result.Failed.Length - result.NeedPublishing.Length, ids.Length));
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
