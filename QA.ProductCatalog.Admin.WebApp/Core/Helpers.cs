using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Newtonsoft.Json;
using QA.Core.DPC.UI;
using QA.Core.DPC.UI.Controls;
using QA.Core.DPC.UI.Controls.EntityEditorControls;
using QA.Core.Models.UI;
using Swashbuckle.AspNetCore.Swagger;
using HtmlString = Microsoft.AspNetCore.Html.HtmlString;
using Microsoft.AspNetCore.WebUtilities;
using Group = QA.Core.DPC.UI.Controls.Group;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
    public static class Helpers
    {
        public static HtmlString LinkButton(this IHtmlHelper helper, string text, string id, string @class = "")
        {
            var template = @"<span id='{2}' class='linkButton actionLink'>
                <a href='javascript:void(0);'>
                    <span class='icon {1}'>&nbsp;&nbsp;&nbsp;&nbsp;</span>
                    <span class='text'>{3}</span>
                </a>
            </span>";
            return new HtmlString(string.Format(template, "/images/icons/0.gif",
                @class,
                id,
                text));
        }

        public static string GetUniqueId(this IHtmlHelper helper, string key)
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
        
        public static string GetJson(this IHtmlHelper helper, object model, bool encode = false)
        {
            string result = "";

            if (model != null)
            {
                result = JsonConvert.SerializeObject(model);

                if (encode && result != null)
                {
                    result = result.Replace('\"', '\'');
                    result = JavaScriptEncoder.Default.Encode(result);
                }
            }

            return result;
        }
        
        public static async Task<string> RenderRazorViewToString(this Controller controller, ICompositeViewEngine viewEngine, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);
                var viewContext = new ViewContext(
                    controller.ControllerContext, 
                    viewResult.View, 
                    controller.ViewData, 
                    controller.TempData, 
                    sw, 
                    new HtmlHelperOptions()
                );
                await viewResult.View.RenderAsync(viewContext);
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
            return ReplaceNotesIfNeeded(model, new HtmlString((htmlEncode && text != null) ? HtmlEncoder.Default.Encode(text) : text));
        }

        public static HtmlString Current(this IUrlHelper helper, Dictionary<string, string> values)
        {
            var uri = helper.ActionContext.HttpContext.Request.GetDisplayUrl();
            foreach (var pair in values)
            {
                var re = new Regex($"{pair.Key}=([^&])*");
                var res = $"{pair.Key}={pair.Value}";
                if (re.IsMatch(uri))
                {
                    uri = re.Replace(uri, res);
                }
                else
                {
                    uri += "&" + res;
                }           
            }

            return new HtmlString(uri);
        }

        public static HtmlString VersionedContent(this IUrlHelper urlHelper, string contentPath)
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

            return new HtmlString(urlHelper.Content(contentPath));
        }
        
        public static IHtmlContent DisplayForUIElement<TModel>(this IHtmlHelper<TModel> helper, UIElement element)
        {
            switch (element)
            {
                case ActionLink actionLink:
                    return helper.DisplayFor(x => actionLink);
                case Label label:
                    return helper.DisplayFor(x => label);
                case DisplayField df:
                    return helper.DisplayFor(x => df);
                case GroupGridView ggv:
                    return helper.DisplayFor(x => ggv);
                case TabStrip tabStrip:
                    return helper.DisplayFor(x => tabStrip);
                case StackPanel stackPanel:
                    return helper.DisplayFor(x => stackPanel);
                case Group g:
                    return helper.DisplayFor(x => g);
                case PropertyDisplay pd:
                    return helper.DisplayFor(x => pd);
                case EntityEditor ee:
                    return helper.DisplayFor(x => ee);
                case EntityCollection ec:
                    return helper.DisplayFor(x => ec);
                case DocumentReference dc:
                    return helper.DisplayFor(x => dc);
                case Switcher sw:
                    return helper.DisplayFor(x => sw);
                case Template tmpl:
                    return helper.DisplayFor(x => tmpl);
                case ArticleInfo ai:
                    return helper.DisplayFor(x => ai);
                case ArticleCollection ac:
                    return helper.DisplayFor(x => ac);
                default:
                    return new HtmlString("");
            }
        }
    }
}