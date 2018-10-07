using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Newtonsoft.Json;
using VeraDemoNet.CustomAuthentication.CustomAuthenticationMVC.CustomAuthentication;
using VeraDemoNet.Models;

namespace VeraDemoNet
{
    // From https://www.c-sharpcorner.com/article/custom-authentication-with-asp-net-mvc/
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)  
        {  
            HttpCookie authCookie = Request.Cookies["Cookie1"];  
            if (authCookie != null)  
            {  
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);  
                var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);  
  
                var principal = new CustomPrincipal(authTicket.Name);  
  
                principal.UserId = serializeModel.UserId;  
                principal.UserName = serializeModel.UserName;  
                principal.BlabName = serializeModel.BlabName;  
                principal.RealName = serializeModel.RealName;
  
                HttpContext.Current.User = principal;  
            }  
  
        }  
    }
}
