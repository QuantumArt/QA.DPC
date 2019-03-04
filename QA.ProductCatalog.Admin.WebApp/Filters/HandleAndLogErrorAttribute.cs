//using QA.Core;
//using QA.Core.Logger;
//
//namespace System.Web.Mvc
//{
//    public class HandleAndLogErrorAttribute : HandleErrorAttribute
//    {
//        public static readonly string DataKey = "HandleAndLogErrorAttribute.data";
//
//        public override void OnException(ExceptionContext filterContext)
//        {
//            var logger = ObjectFactoryBase.Resolve<ILogger>();
//            var exception = filterContext.Exception;
//            try
//            {
//                if (exception != null)
//                {
//                    var innterText = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
//
//                    logger.ErrorException("ошибка во время выполнения "
//                        + filterContext.RouteData.Values["controller"] + " " + filterContext.RouteData.Values["action"], exception);
//
//                    filterContext.RouteData.DataTokens[DataKey] = exception.Flat();
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.ErrorException("logging error", ex);                
//            }
//
//            base.OnException(filterContext);
//        }
//    }
//}