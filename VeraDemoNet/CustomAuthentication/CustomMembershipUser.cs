using System;
using System.Web.Security;
using VeraDemoNet.DataAccess;

namespace VeraDemoNet.CustomAuthentication
{
    namespace CustomAuthenticationMVC.CustomAuthentication  
    {  
        public class CustomMembershipUser : MembershipUser  
        {  
            public string BlabName { get; set; }  
            public string RealName { get; set; }  
 
            public CustomMembershipUser(User user):base("CustomMembership", user.UserName, user.UserName, string.Empty, string.Empty, string.Empty, true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now)  
            {  
                BlabName = user.BlabName;
                RealName = user.RealName;  
            }  
        }  
    }  
}