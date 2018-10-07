using System.Linq;  
using System.Security.Principal;
using System.Web.Security;

namespace VeraDemoNet.CustomAuthentication
{
    namespace CustomAuthenticationMVC.CustomAuthentication  
    {  
        public class CustomPrincipal : IPrincipal  
        {  
            #region Identity Properties  
  
            public int UserId { get; set; }
            public string UserName { get; set; }  
            public string BlabName { get; set; }  
            public string RealName { get; set; }  
            #endregion  
  
            public IIdentity Identity  
            {  
                get; private set;  
            }  
  
            public bool IsInRole(string role)
            {
                return true;
            }  
  
            public CustomPrincipal(string username)  
            {  
                Identity = new GenericIdentity(username);  
            }  
        }  
    }  
}