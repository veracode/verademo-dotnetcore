using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
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
        [Column("is_admin")] public bool IsAdmin { get; set; }

        public static User Create(string userName, string blabName, string realName, bool isAdmin = false)
        {
            var password = Md5Hash(userName);
            var createdAt = DateTime.Now;

            return new User(userName, password, createdAt, null, blabName, realName, isAdmin);
        }

        public User()
        {
            
        }

        public User(string userName, string password, DateTime createdAt, DateTime? lastLogin, string blabName, string realName, bool isAdmin) 
        {
            UserName = userName;
            Password = password;
            PasswordHint = password;
            CreatedAt = createdAt;
            LastLogin = lastLogin;
            BlabName = blabName;
            RealName = realName;
            IsAdmin = isAdmin;
        }

        protected static string Md5Hash(string input)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(input))
            {
                return sb.ToString();
            }

            using (MD5 md5 = MD5.Create())
            {
                var retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(input));

                foreach (var t in retVal)
                {
                    sb.Append(t.ToString("x2"));
                }
            }

            return sb.ToString();
        }
    }
}