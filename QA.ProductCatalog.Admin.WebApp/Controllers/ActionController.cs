using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Admin.WebApp.Binders;
using Unity;
using QA.ProductCatalog.Admin.WebApp.Filters;
using QA.ProductCatalog.ContentProviders;
using ActionContext = QA.Core.ProductCatalog.Actions.ActionContext;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using A = QA.Core.ProductCatalog.Actions.Actions.Abstract;
using NLog;
using NLog.Fluent;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;


namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [Route("action")]
    public class ActionController : BaseController
    {
        private readonly Func<string, string, IAction> _getAction;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
        public ActionController(Func<string, string, IAction> getAction)
        {
            _getAction = getAction;
        }
        
        [Route("{command}")]
        [RequireCustomAction]
        public async Task<ActionResult> Action(string command, [ModelBinder(typeof(ActionContextModelBinder))] ActionContext context, string adapter)
        {
            if (!ModelState.IsValid)
            {
                return Error(ModelState);
            }

            try
            {
                var action = _getAction(command, adapter);
                ActionTaskResult result;

                if (action is IAsyncAction asyncAction)
                {
                    result = await asyncAction.Process(context);
                }
                else
                {
                    result = action.Process(context);
                }
                Logger.Debug()
                    .Message($"Action '{command}' succeeded", command)
                    .Property("context", context)
                    .Property("adapter", adapter)
                    .Write();
                return Info(result?.ToString());
            }
            catch (ActionException ex)
            {
                Logger.Debug()
                    .Message($"Action '{command}' failed", command)
                    .Property("context", context)
                    .Property("adapter", adapter)
                    .Exception(ex)
                    .Write();
                return Error(ex);
            }
            catch (ResolutionFailedException ex)
            {
                Logger.Error()
                    .Message($"Action '{command}' not found", command)
                    .Exception(ex)
                    .Write();
                return Error("Handler is not found for command: " + command);
            }
        }
    }
}