﻿using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using QA.Validation.Xaml.Extensions.Rules;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    class RemoteValidationContextModelBinder:IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            string body;

            if (controllerContext.HttpContext.Request.InputStream.Position > 0)
                controllerContext.HttpContext.Request.InputStream.Position = 0;

            using (var reader=new StreamReader(controllerContext.HttpContext.Request.InputStream))
            {
                body = reader.ReadToEnd();
            }

            return new JavaScriptSerializer().Deserialize<RemoteValidationContext>(body);
        }
    }
}
