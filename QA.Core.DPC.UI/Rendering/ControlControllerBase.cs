#if !NETSTANDARD
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QA.Core.Logger;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    /// <summary>
    /// Контроллер для контрола
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ControlControllerBase<T> : Controller
        where T : UIElement
    {
        private ILogger _logger;
        private const string _uIElementKey = "uielement-key";

        public static string UIElementKey
        {
            get { return _uIElementKey; }
        } 


        public ControlControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            return null;
        }
    }
}
#endif
