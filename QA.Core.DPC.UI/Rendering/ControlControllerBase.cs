using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
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


        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            var elem = requestContext.RouteData.DataTokens[_uIElementKey] as T;
            base.Initialize(requestContext);
        }
    }
}
