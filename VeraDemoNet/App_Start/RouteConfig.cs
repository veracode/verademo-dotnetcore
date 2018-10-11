using System.Web.Mvc;
using System.Web.Routing;

namespace VeraDemoNet
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Blabbers",
                url: "Blabbers",
                defaults: new { controller = "Blab", action = "Blabbers" }
            );

            routes.MapRoute(
                name: "Feed",
                url: "Feed",
                defaults: new { controller = "Blab", action = "Feed" }
            );

            routes.MapRoute(
                name: "Tools",
                url: "Tools",
                defaults: new { controller = "Tools", action = "Tools" }
            );

            routes.MapRoute(
                name: "Reset",
                url: "Reset",
                defaults: new { controller = "Reset", action = "Reset" }
            );

            routes.MapRoute(
                name: "DownloadProfileImage",
                url: "DownloadProfileImage",
                defaults: new { controller = "Account", action = "DownloadProfileImage" }
            );

            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Register",
                url: "Register",
                defaults: new { controller = "Account", action = "Register" }
            );

            routes.MapRoute(
                name: "Profile",
                url: "Profile",
                defaults: new { controller = "Account", action = "Profile" }
            );

            routes.MapRoute(
                name: "PasswordHint",
                url: "PasswordHint",
                defaults: new { controller = "Account", action = "PasswordHint" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
