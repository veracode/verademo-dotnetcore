namespace VeraDemoNet.DataAccess
{
    public class BasicUser
    {
        public string UserName { get; set; }
        public string BlabName { get; set; }
        public string RealName { get; set; }
        public bool IsAdmin { get; set; }

        public BasicUser()
        {
                
        }

        public BasicUser(string userName, string blabName, string realName)
        {
            UserName = userName;
            BlabName = blabName;
            RealName = realName;  
        }  
    }
}