using System.Web;
using System.Web.Mvc;
using VeraDemoNet.CustomAuthentication.CustomAuthenticationMVC.CustomAuthentication;

namespace VeraDemoNet.CustomAuthentication
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute  
    {  
        protected virtual CustomPrincipal CurrentUser => HttpContext.Current.User as CustomPrincipal;

        protected override bool AuthorizeCore(HttpContextBase httpContext)  
        {  
            return (CurrentUser != null);  
        }  



        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)  
        {  
            RedirectToRouteResult routeData;  

            if(CurrentUser == null)  
            {  
                routeData = new RedirectToRouteResult  (
                    new System.Web.Routing.RouteValueDictionary  
                        (new  { controller = "Account",  
                                action = "Login", 
                                ReturnUrl = filterContext.HttpContext.Request.RawUrl})
                    );
                
            }  
            else  
            {  
                routeData = new RedirectToRouteResult  
                (new System.Web.Routing.RouteValueDictionary  
                (new  
                    {  
                        controller = "Error",  
                        action = "AccessDenied"  
                    }  
                ));  
            }  

            filterContext.Result = routeData;  
        }  
    }  
}