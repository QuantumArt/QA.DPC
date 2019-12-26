using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using QA.Core.ProductCatalog.Actions.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class BaseController: Controller
    {
        protected ActionResult Error(ModelStateDictionary modelstate)
        {
            var errors = from fv in modelstate
                from e in fv.Value.Errors
                select new { Field = fv.Key, Message = e.ErrorMessage };

            return Json(new {Type = "Error", Text = "Validation", ValidationErrors = errors});
        }

        protected ActionResult Error(ActionException exception)
        {
            if (exception.InnerExceptions.Any())
            {
                var sb = new StringBuilder("Products have not been processed:");

                foreach (var exception1 in exception.InnerExceptions)
                {
                    var ex = (ProductException) exception1;
                    sb.AppendLine();

                    var exText = ex.Message;

                    if (ex.InnerException != null)
                        exText += ". " + ex.InnerException.Message;

                    sb.AppendFormat("{0}: {1}", ex.ProductId, exText);
                }

                return Error(sb.ToString());
            }
            else
            {
                return Error(exception.Message);
            }
        }

        protected new ActionResult Json(object data)
        {
            return new ContentResult { Content = JsonConvert.SerializeObject(data), ContentType = "application/json"};            
        }

        protected ActionResult Error(string text)
        {
            return Json(new { Type = "Error", Text = text });
        }
        
        [Produces("application/json")]
        protected ActionResult Info(string text)
        {
            return Json(new { Type = "Info", Text = text });
        }
    }
}