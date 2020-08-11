#if !NETSTANDARD
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
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
#endif
