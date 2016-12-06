using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace QA.ProductCatalog.Admin.WebApp.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterScripts(bundles);

            bundles.Add(new StyleBundle("~/content/site")
                .Include("~/content/site.css"));

            bundles.Add(new Bundle("~/content/KendoUI/KendoUIStyles")
                .Include("~/content/KendoUI/kendo.common.min.css")
                .Include("~/content/KendoUI/kendo.default.min.css"));

			bundles.Add(new Bundle("~/content/codemirror")
				.Include("~/Scripts/codemirror/lib/codemirror.css")
				.Include("~/Scripts/codemirror/addon/fold/foldgutter.css")
				.Include("~/Content/XmlViewer.css"));
        }

        private static void RegisterScripts(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/scripts/jquery/jquery-1.11.1.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/knockout")
              .Include("~/scripts/knockout/knockout-3.4.1.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/scripts")
                .Include("~/scripts/vanilla.helpers.js")
                .Include("~/scripts/jq.trottle.js")
                .Include("~/scripts/jquery.scrollto.js")
                .Include("~/scripts/json2.js")
                .Include("~/scripts/pmrpc.js")
                .Include("~/scripts/qp/qp8backendApi.interaction.js")
                .Include("~/scripts/qp/qa.utils.js")
                .Include("~/scripts/qp/qa.integration.js")
                );

			bundles.Add(new ScriptBundle("~/bundles/jqueryunobtrusive").Include("~/Scripts/jquery.unobtrusive*"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror")
                .Include("~/scripts/codemirror/lib/codemirror.js")
                .Include("~/scripts/codemirror/addon/fold/xml-fold.js")
				.Include("~/scripts/codemirror/addon/fold/brace-fold.js")
                .Include("~/scripts/codemirror/addon/edit/matchtags.js")
                .Include("~/scripts/codemirror/mode/xml/xml.js")
				.Include("~/scripts/codemirror/mode/javascript/javascript.js")
                .Include("~/scripts/codemirror/addon/fold/foldcode.js")
                .Include("~/scripts/codemirror/addon/fold/foldgutter.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/product/index")
               .Include("~/scripts/product/index.js")
               .Include("~/scripts/product/tabstrip.js")
               );

            bundles.Add(new Bundle("~/bundles/KendoUI")
                .Include("~/scripts/KendoUI/jquery.min.js")
                .Include("~/scripts/KendoUI/kendo.web.min.js")
				.Include("~/scripts/KendoUI/kendo.culture.ru-RU.min.js")
				.Include("~/scripts/KendoUI/kendo.messages.ru-RU.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/Schedule")
				.Include("~/scripts/jqcron.js")
				.Include("~/scripts/jqcron.en.js")
				.Include("~/Scripts/Schedule.js"));
        }
    }
}