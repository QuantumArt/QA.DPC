using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.Resources;
using QA.Core.Models.Configuration;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ResourceController: Controller
    {
        [RequireCustomAction]
        public ActionResult GetUpdateEnum()
        {
            return Json(new[] {
                new { Value = (int)UpdatingMode.Update, Title = ControlStrings.UpdateOrCreate },
                new { Value = (int)UpdatingMode.Ignore, Title = ControlStrings.Ignore },                
            });
        }
        
        [RequireCustomAction]
        public ActionResult GetPublishEnum()
        {
            return Json(new[] {
                new { Value = (int)PublishingMode.Publish, Title = ControlStrings.Publish },                     
                new { Value = (int)PublishingMode.SkipRecursive, Title = ControlStrings.DontPublish },
            });
        }

        [RequireCustomAction]
        public ActionResult GetPreloadEnum()
        {
            return Json(new[] {
                new { Value = (int)PreloadingMode.None, Title = ControlStrings.IgnoreLoading },
                new { Value = (int)PreloadingMode.Eager, Title = ControlStrings.EagerLoading },
                new { Value = (int)PreloadingMode.Lazy, Title = ControlStrings.LazyLoading },                      
            });
        }
        
        [RequireCustomAction]
        public ActionResult GetDeleteEnum()
        {
            return Json(new[] {
                new { Value = (int)DeletingMode.Keep, Title = ControlStrings.DontRemove },
                new { Value = (int)DeletingMode.Delete, Title = ControlStrings.Remove },                     
            });
        }

        [RequireCustomAction]
        public ActionResult GetCloneEnum()
        {
            return Json(new[] {
                new { Value = (int)CloningMode.Ignore, Title = ControlStrings.SetNull },
                new { Value = (int)CloningMode.UseExisting, Title = ControlStrings.UseExisting },
                new { Value = (int)CloningMode.Copy, Title = ControlStrings.Clone },
            });
        }

    }
}