using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Resources;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Validation;
using QA.Validation.Xaml.Extensions.Rules;
using System;
using System.Linq;
using Unity;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [AllowAnonymous]
    [Route("RemoteValidation")]
    public class RemoteValidationController : Controller
    {
        private readonly Func<string, IRemoteValidator2> _validationFactory;
        private readonly IIdentityProvider _identityProvider;
        private readonly IFactory _consolidationFactory;

        public RemoteValidationController(Func<string, IRemoteValidator2> validationFactory, IIdentityProvider identityProvider, IFactory consolidationFactory)
        {
            _validationFactory = validationFactory;
            _identityProvider = identityProvider;
            _consolidationFactory = consolidationFactory;
        }
        
        [Route("{validatorKey}")]
        public ActionResult Validate(string validatorKey, [FromBody] RemoteValidationContext context)
        {
            HttpContextUserProvider.ForcedUserId = 1;
            var result = new RemoteValidationResult();
            try
            {
                var errorMessage = _consolidationFactory.Validate(context.CustomerCode);

                if (errorMessage != null)
                {
                    result.Messages.Add(errorMessage);
                }
                else
                {
                    result = _validationFactory(validatorKey).Validate(context, result);
                }
            }
            catch (ResolutionFailedException ex)
            {
                result.Messages.Add("Validator " + validatorKey + " is not registered: " + ex.Message);
            }
            catch (ValidationException ex)
            {
                result.Messages.Add(ex.Message);
            }
            HttpContextUserProvider.ForcedUserId = 0;

            return Json(result);
        }
    }
}
