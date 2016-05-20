using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class ControlRenderer
    {
        public void RenderControl(UIElement element, HtmlHelper helper)
        {
            var rw=new RouteValueDictionary();
            
            rw[ControlControllerBase<UIElement>.UIElementKey] = element;
                       
        }
    }
}
