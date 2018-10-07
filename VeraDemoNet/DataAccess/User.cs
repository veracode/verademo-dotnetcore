using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VeraDemoNet.DataAccess
{
    public class User
    {
        public string UserName { get; set; }
        [Column("blab_name")] public string BlabName { get; set; }
        [Column("real_name")] public string RealName { get; set; }
        [Column("password")] public string Password { get; set; }
        [Column("password_hint")] public string PasswordHint { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; }
        [Column("last_login")] public DateTime? LastLogin { get; set; }

        public static User Create(string userName, string blabName, string realName)
        {
            var password = userName;
            var createdAt = DateTime.Now;
            DateTime? lastLogin = null;

            return new User(userName, password, createdAt, lastLogin, blabName, realName);
        }

        public User()
        {
            
        }

        public User(string userName, string password, DateTime createdAt, DateTime? lastLogin, string blabName, string realName) 
        {
            UserName = userName;
            Password = md5(password);
            PasswordHint = password;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            BlabName = blabName;
            RealName = realName;
        }

        private static string md5(string val)
        {
            var sb = new StringBuilder();

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                var retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(val));
                
                foreach (var t in retVal)
                {
                    sb.Append(t.ToString("x2"));
                }
            }
           
            return sb.ToString();
        }
    }
}