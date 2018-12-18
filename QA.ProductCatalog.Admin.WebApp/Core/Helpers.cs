using System.IO;
using QA.Core.DPC.UI.Controls;
using QA.Core.Models.UI;
using System.ComponentModel;

namespace System.Web.Mvc
{
    public static class Helpers
    {
        public static IHtmlString LinkButton(this HtmlHelper helper, string text, string id, string @class = "")
        {
            var template = @"<span id='{2}' class='linkButton actionLink'>
                <a href='javascript:void(0);'>
                    <span class='icon {1}'>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                    <span class='text'>{3}</span>
                </a>
            </span>";
            return MvcHtmlString.Create(string.Format(template, "/content/img/icons/0.gif",
                @class,
                id,
                text));
        }

        public static string GetUniqueId(this HtmlHelper helper, string key)
        {
            if (key == null)
                return null;

            var hash = "_" + key.GetHashCode();

            var cKey = "unique_id_key" + hash;

            var v = helper.ViewContext.HttpContext.Items[cKey];
            if (v is int)
            {
                helper.ViewContext.HttpContext.Items[cKey] = ((int)v) + 1;
                hash += "_" + v;
            }
            else
            {
                helper.ViewContext.HttpContext.Items[cKey] = 0;
            }

            return hash;
        }

        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static HtmlString ReplaceNotesIfNeeded(this DependencyObject model, HtmlString text)
        {
            var notesProcessor = NotesProcessorBase.GetNotesProcessor(model);

            return notesProcessor == null ? text : new HtmlString(notesProcessor.ProcessTextWithNotes(text.ToString()));
        }

        public static HtmlString ReplaceNotesIfNeeded(this DependencyObject model, string text, bool htmlEncode = true)
        {
            return ReplaceNotesIfNeeded(model, new HtmlString(htmlEncode ? HttpUtility.HtmlEncode(text) : text));
        }

        public static MvcHtmlString Current(this UrlHelper helper, object substitutes)
        {
            var uri = helper.RequestContext.HttpContext.Request.Url;
            var uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
       
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(substitutes.GetType()))
            {
                var value = property.GetValue(substitutes) as string;

                if (value == null)
                {
                    query.Remove(property.Name);
                }
                else
                {
                    query[property.Name] = value;
                }
            }

            uriBuilder.Query = query.ToString();

            return new MvcHtmlString(uriBuilder.ToString());
        }

        public static MvcHtmlString VersionedContent(this UrlHelper urlHelper, string contentPath)
        {
            int revision = typeof(Helpers).Assembly.GetName().Version.Revision;

            if (contentPath.Contains("?"))
            {
                contentPath += "&v=" + revision;
            }
            else
            {
                contentPath += "?v=" + revision;
            }

            return new MvcHtmlString(urlHelper.Content(contentPath));
        }
    }
}