using System.ComponentModel.DataAnnotations.Schema;

namespace VeraDemoNet.Models
{
    namespace CustomAuthentication.CustomAuthenticationMVC.CustomAuthentication  
    {  
        public class VeraDemoUser
        {
            public string UserName { get; set; }
            public string BlabName { get; set; }
            public string RealName { get; set; }
            public bool IsAdmin { get; set; }

            public VeraDemoUser()
            {
                
            }

            public VeraDemoUser(string userName, string blabName, string realName)
            {
                UserName = userName;
                BlabName = blabName;
                RealName = realName;  
            }  
        }  
    }  
}