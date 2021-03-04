﻿using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Verademo.Data;
using Verademo.Models;

namespace Verademo.Controllers
{
    public abstract class AuthControllerBase : Controller
    {
        protected BasicUser LoginUser(string userName, string passWord)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            using (var dbContext = new ApplicationDbContext())
            {
                var found = dbContext.Database.SqlQuery<BasicUser>(
                    "select username, real_name as realname, blab_name as blabname, is_admin as isadmin from users where username ='"
                    + userName + "' and password='" + Md5Hash(passWord) + "';").ToList();

                if (found.Count != 0)
                {
                    HttpContext.Session.SetString("username", userName);
                    return found[0];
                }
            }

            return null;
        }

        protected string GetLoggedInUsername()
        {
            return HttpContext.Session.GetString("username");
        }

        protected void LogoutUser()
        {
            HttpContext.Session.SetString("username", "");
        }

        protected bool IsUserLoggedIn()
        {
            return string.IsNullOrEmpty(HttpContext.Session.GetString("username")) == false;
        }

        protected RedirectToRouteResult RedirectToLogin(string targetUrl)
        {
            return new RedirectToRouteResult(
                new RouteValueDictionary
                (new
                {
                    controller = "Account",
                    action = "Login",
                    ReturnUrl = HttpContext.Request.QueryString
                }));
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