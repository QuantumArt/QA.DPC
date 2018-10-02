using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Binders
{
    /// <summary>
    /// Binds model from JSON using Newtonsoft.Json serializer.
    /// </summary>
    public class JsonModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (!IsJsonRequest(controllerContext))
            {
                throw new HttpRequestValidationException("Request type is not 'application/json'");
            }

            Stream stream = controllerContext.HttpContext.Request.InputStream;

            if (stream.CanSeek && stream.Position > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            
            object model;

            using (var reader = new StreamReader(stream))
            {
                model = JsonSerializer.CreateDefault().Deserialize(reader, bindingContext.ModelType);
            }

            TryValidateModel.Invoke(controllerContext.Controller, new[] { model, bindingContext.ModelName });

            return model;
        }

        protected static readonly MethodInfo TryValidateModel = typeof(Controller)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(mi => mi.Name == "TryValidateModel" && mi.GetParameters().Length == 2);

        protected static bool IsJsonRequest(ControllerContext controllerContext)
        {
            string contentType = controllerContext.HttpContext.Request.ContentType;
            return contentType.Contains("application/json");
        }
    }

    /// <summary>
    /// Handles what is possible with <see cref="JsonModelBinder"/> and the rest with <see cref="DefaultModelBinder"/>
    /// </summary>
    public class JsonDefaultModelBinder : JsonModelBinder
    {
        private readonly DefaultModelBinder _defaultBinder;

        public JsonDefaultModelBinder()
        {
            _defaultBinder = new DefaultModelBinder();
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (!IsJsonRequest(controllerContext) || ShouldUseDefaultBinder(bindingContext.ModelType))
            {
                return _defaultBinder.BindModel(controllerContext, bindingContext);
            }
            return base.BindModel(controllerContext, bindingContext);
        }

        /// <remarks>
        /// Json.Net cannot handle requests to actions which accept multiple 
        /// primitive parameters instead of single object parameter.
        /// It tries to convert the whole json to primitive types of each parameter and fails.
        /// Such requests must be handled by default model binder.
        /// </remarks>
        private static bool ShouldUseDefaultBinder(Type type)
        {
            return IsValueTypeOrString(type)
                || type.IsArray // Array of primitive types
                   && IsValueTypeOrString(type.GetElementType())
                || type.IsGenericType // IEnumerable of primitive types
                   && typeof(IEnumerable).IsAssignableFrom(type)
                   && IsValueTypeOrString(type.GetGenericArguments()[0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValueTypeOrString(Type type)
        {
            // Nullable<> is value type
            return type == typeof(string) || type.IsValueType;
        }
    }
}