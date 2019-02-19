using System;
using System.Web.Mvc;
using QA.Validation.Xaml.Extensions.Rules;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation;
using Unity;
using Unity.Exceptions;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [AllowAnonymous]
    public class RemoteValidationController : Controller
    {
        private readonly Func<string, IRemoteValidator2> _validationFactory;

        public RemoteValidationController(Func<string, IRemoteValidator2> validationFactory)
        {
            UserProvider.ForcedUserId = 1;
            _validationFactory = validationFactory;
        }

        public ActionResult Validate(string validatorKey, RemoteValidationContext context)
        {
            var result = new RemoteValidationResult();
            try
            {
                result = _validationFactory(validatorKey).Validate(context, result);
            }
            catch (ResolutionFailedException ex)
            {
                result.Messages.Add("Validator " + validatorKey + " is not registered: " + ex.Message);
            }
            catch (ValidationException ex)
            {
                result.Messages.Add(ex.Message);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
