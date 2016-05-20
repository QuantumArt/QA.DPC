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
			var result = new ValidationContext();
			try
			{
				var validator = _validationFactory(validatorKey);
				validator.Validate(context, ref result);
			}
			catch (ResolutionFailedException)
			{
				result.AddErrorMessage("Validator " + validatorKey + " is not registered");
			}
			catch (ValidationException ex)
			{
				result.AddErrorMessage(ex.Message);
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}
}
