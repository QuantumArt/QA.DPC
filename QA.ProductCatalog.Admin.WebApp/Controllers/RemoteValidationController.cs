using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Validation.Xaml.Extensions.Rules;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation;
using Unity;
using Unity.Exceptions;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [AllowAnonymous]
    [Route("RemoteValidation")]
    public class RemoteValidationController : Controller
    {
        private readonly Func<string, IRemoteValidator2> _validationFactory;

        public RemoteValidationController(Func<string, IRemoteValidator2> validationFactory)
        {
            HttpContextUserProvider.ForcedUserId = 1;
            _validationFactory = validationFactory;
        }
        
        [Route("{validatorKey}")]
        public ActionResult Validate(string validatorKey, [FromBody] RemoteValidationContext context)
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

            return Json(result);
        }
    }
}
