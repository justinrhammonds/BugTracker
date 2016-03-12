using System.Web;
using System.Web.Optimization;

namespace BugTracker
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/theme/start-addons").Include(
                    "~/Scripts/theme/jquery.easing.1.3.min.js",
                      "~/Scripts/theme/SmoothScroll.js",
                      "~/Scripts/theme/wow.min.js",
                      "~/Scripts/theme/jquery.stellar.min.js",
                      "~/Scripts/theme/waypoints.min.js",
                      "~/Scripts/theme/jquery.counterup.min.js",
                      "~/Scripts/theme/jquery.magnific-popup.min.js",
                      "~/Scripts/theme/jquery.sticky.js"));

            bundles.Add(new ScriptBundle("~/bundles/theme/dash-addons").Include(
                    "~/Scripts/theme/detect.js",
                      "~/Scripts/theme/fastclick.js",
                      "~/Scripts/theme/jquery.slimscroll.min.js",
                      "~/Scripts/theme/jquery.blockUI.js",
                      "~/Scripts/theme/waves.js",
                      "~/Scripts/theme/wow.min.js",
                      "~/Scripts/theme/jquery.nicescroll.js",
                      "~/Scripts/theme/jquery.scrollTo.min.js",
                      "~/Scripts/theme/jquery.core.js",
                      "~/Scripts/theme/jquery.app.js"));
            bundles.Add(new ScriptBundle("~/bundles/theme/dataTables/dash-addons").Include(
                    "~/Scripts/theme/dataTables/jQuery.dataTables.min.js",
                      "~/Scripts/theme/dataTables/dataTables.bootstrap.js",
                      "~/Scripts/theme/dataTables/dataTables.buttons.min.js",
                      "~/Scripts/theme/dataTables/buttons.bootstrap.min.js",
                      "~/Scripts/theme/dataTables/jszip.min.js",
                      "~/Scripts/theme/dataTables/pdfmake.min.js",
                      "~/Scripts/theme/dataTables/vfs_fonts.js",
                      "~/Scripts/theme/dataTables/buttons.html5.min.js",
                      "~/Scripts/theme/dataTables/buttons.print.min.js",
                      "~/Scripts/theme/dataTables/dataTables.responsive.min.js",
                      "~/Scripts/theme/dataTables/responsive.bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/theme/dashboard").Include(
                       "~/Content/theme/core.css",
                       "~/Content/theme/components.css",
                       "~/Content/theme/icons.css",
                       "~/Content/theme/pages.css",
                       "~/Content/theme/menu.css",
                       "~/Content/theme/responsive.css"));
            bundles.Add(new StyleBundle("~/Content/theme/dataTables/dashboard").Include(
                        "~/Content/theme/dataTables/jQuery.dataTables.min.css",
                        "~/Content/theme/dataTables/buttons.bootstrap.min.css",
                        "~/Content/theme/dataTables/responsive.bootstrap.min.css"));
            bundles.Add(new StyleBundle("~/Content/theme/start").Include(
                       "~/Content/theme/animate.css",
                       "~/Content/theme/magnific-popup.css",
                       "~/Content/theme/themify-icons.css",
                       "~/Content/theme/style.css"));
        }
    }
}
