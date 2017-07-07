using System;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [AllowAnonymous]
    public class RemoteValidationController : Controller
    {
        private readonly Func<string, IRemoteValidator> _validationFactory;

        public RemoteValidationController(Func<string, IRemoteValidator> validationFactory)
        {
            UserProvider.ForcedUserId = 1;
            _validationFactory = validationFactory;
        }

        public ActionResult Validate(string validatorKey, RemoteValidationContext context)
        {
            ValidationContextBase result = new RemoteValidationResult();
            try
            {
                result = ValidateInternal(validatorKey, context);
            }
            catch (ResolutionFailedException)
            {
                result.Messages.Add("Validator " + validatorKey + " is not registered");
            }
            catch (ValidationException ex)
            {
                result.Messages.Add(ex.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private ValidationContextBase ValidateInternal(string validatorKey, RemoteValidationContext context)
        {
            ValidationContextBase result;
            var validator = _validationFactory(validatorKey);
            var remoteValidator = validator as IRemoteValidator2;

            if (remoteValidator != null)
            {
                result = remoteValidator.Validate(context, new RemoteValidationResult());
            }
            else
            {
                var exactResult = new ValidationContext();
                validator.Validate(context, ref exactResult);
                result = exactResult;
            }
            return result;
        }
    }
}
