using System.Web;
using System.Web.Optimization;

namespace WebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;   //enable CDN support

            //add link to jquery on the CDN
            var jqueryCdnPath = "https://code.jquery.com/jquery-3.2.1.min.js";
            var bootstrapJSCdnPath = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js";
            var bootstrapCSSCdnPath = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css";

            bundles.Add(new StyleBundle("~/Content/css", bootstrapCSSCdnPath).Include(
                "~/Content/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/jqx.base.css",
                "~/Content/jqx.arctic.css",
                "~/Content/jqx.black.css",
                "~/Content/jqx.bootstrap.css",
                "~/Content/jqx.classic.css",
                "~/Content/jqx.darkblue.css",
                "~/Content/jqx.energyblue.css",
                "~/Content/jqx.fresh.css",
                "~/Content/jqx.highcontrast.css",
                "~/Content/jqx.metro.css",
                "~/Content/jqx.metrodark.css",
                "~/Content/jqx.office.css",
                "~/Content/jqx.orange.css",
                "~/Content/jqx.shinyblack.css",
                "~/Content/jqx.summer.css",
                "~/Content/jqx.web.css",
                "~/Content/jqx.ui-darkness.css",
                "~/Content/jqx.ui-lightness.css",
                "~/Content/jqx.ui-le-frog.css",
                "~/Content/jqx.ui-overcast.css",
                "~/Content/jqx.ui-redmond.css",
                "~/Content/jqx.ui-smoothness.css",
                "~/Content/jqx.ui-start.css",
                "~/Content/jqx.ui-sunny.css",
                "~/Content/bootswatch.bootstrap.css",
                "~/Content/site.css"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jqwidgets").Include(
            "~/Scripts/jqx-all.js",
            "~/Scripts/globalize.js"
             ));

            bundles.Add(new ScriptBundle("~/bundles/jquery",
                        jqueryCdnPath).Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap", bootstrapJSCdnPath).Include(
                "~/Scripts/bootstrap.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/helpers").Include(
                "~/Scripts/Helpers/distillcompute.js",
                "~/Scripts/Helpers/dictionaryManagement.js",
                "~/Scripts/Helpers/genericWorkflowHelpers.js",
                "~/Scripts/Helpers/Ajax.js"
            ));
        }
    }
}
