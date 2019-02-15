using System.Web.Optimization;

namespace VeraDemoNet
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.11.2.min.js"));
            bundles.Add(new StyleBundle("~/Content/css").
                Include("~/Content/bootstrap.min.css").
                Include("~/Content/bootstrap-theme.min.css").
                Include("~/Content/pwm.css"));
        }
    }
}
