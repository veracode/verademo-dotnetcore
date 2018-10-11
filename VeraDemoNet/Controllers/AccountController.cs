using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using VeraDemoNet.CustomAuthentication;
using VeraDemoNet.CustomAuthentication.CustomAuthenticationMVC.CustomAuthentication;
using VeraDemoNet.DataAccess;
using VeraDemoNet.Models;

namespace VeraDemoNet.Controllers  
{  
    // https://www.c-sharpcorner.com/article/custom-authentication-with-asp-net-mvc/
    public class AccountController : Controller  
    {
        protected readonly log4net.ILog logger;

        public AccountController()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);    
        }

        [HttpGet, ActionName("Login")]  
        public ActionResult GetLogin(string ReturnUrl = "")
        {  
            if (User.Identity.IsAuthenticated)  
            {  
                return GetLogOut();  
            }  
            ViewBag.ReturnUrl = ReturnUrl;  
            return View();  
        }  
  
        [HttpPost, ActionName("Login")]  
        public ActionResult PostLogin(LoginView loginViewModel, string ReturnUrl = "")  
        {

            logger.Info("Entering PostLogin with username " + loginViewModel.UserName + " and target " + ReturnUrl);

            if (ModelState.IsValid)  
            {  
                if (Membership.ValidateUser(loginViewModel.UserName, loginViewModel.Password))  
                {  
                    var user = (CustomMembershipUser)Membership.GetUser(loginViewModel.UserName, false);  
                    if (user != null)  
                    {  
                        CustomSerializeModel userModel = new Models.CustomSerializeModel()  
                        {  
                            UserName = user.UserName,
                            BlabName = user.BlabName,
                            RealName = user.RealName
                        };  
  
                        var userData = JsonConvert.SerializeObject(userModel);  
                        var authTicket = new FormsAuthenticationTicket  
                            (  
                            1, loginViewModel.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData  
                            );  
  
                        var enTicket = FormsAuthentication.Encrypt(authTicket);  
                        var faCookie = new HttpCookie("Cookie1", enTicket);  
                        Response.Cookies.Add(faCookie);  
                    }  
  
                    //if (Url.IsLocalUrl(ReturnUrl))  
                    if (string.IsNullOrEmpty(ReturnUrl))
                    {
                        return RedirectToAction("Feed", "Blab");
                    }

                    /* START BAD CODE */
                    return Redirect(ReturnUrl);
                    /* END BAD CODE */                    
                }  
            }  
            ModelState.AddModelError("", "Something Wrong : UserName or Password invalid ^_^ ");  
            return View(loginViewModel);  
        }  
 
        [HttpGet, ActionName("Logout")]
        public ActionResult GetLogOut()
        {
            InvalidateSession();
            return Redirect(Url.Action("Login", "Account"));
        }

        private void InvalidateSession()
        {
            var cookie = new HttpCookie("Cookie1", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };

            Response.Cookies.Add(cookie);  
  
            FormsAuthentication.SignOut();
        }

        [CustomAuthorize]
        [HttpGet, ActionName("Profile")]
        public ActionResult GetProfile()
        {
            var viewModel = new ProfileViewModel();

            var username = User.Identity.Name;

            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();
                viewModel.Hecklers = RetrieveMyHecklers(connection, username);
                viewModel.Events = RetrieveMyEvents(connection, username);
                PopulateProfileViewModel(connection, username, viewModel);
            }

            return View(viewModel);
        }

        [CustomAuthorize]
        [HttpPost, ActionName("Profile")]
        public ActionResult PostProfile(string realName, string blabName, string userName, HttpPostedFileBase file)
        {
            logger.Info("Entering PostProfile");

            var oldUsername = User.Identity.Name;
            var imageDir = HostingEnvironment.MapPath("~/Images/");

            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();

                var update = connection.CreateCommand();
                update.CommandText = "UPDATE users SET real_name=@realname, blab_name=@blabname WHERE username=@username;";
                update.Parameters.Add(new SqlParameter {ParameterName = "@realname", Value = realName});
                update.Parameters.Add(new SqlParameter {ParameterName = "@blabname", Value = blabName});
                update.Parameters.Add(new SqlParameter {ParameterName = "@username", Value = oldUsername});

                var result = update.ExecuteNonQuery();

                if (result == 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return new JsonResult
                    {
                        Data = JsonConvert.DeserializeObject("{\"message\": \"<script>alert('An error occurred, please try again.');</script>\"}")
                    };
                }
            }

            if (userName != oldUsername)
            {
                if (UsernameExists(userName))
                {
                    Response.StatusCode = (int) HttpStatusCode.Conflict;
                    return new JsonResult
                    {
                        Data = JsonConvert.DeserializeObject(
                            "{\"message\": \"<script>alert('That username already exists. Please try another.');</script>\"}")
                    };
                }

                if (!UpdateUsername(oldUsername, userName))
                {
                    Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    return new JsonResult
                    {
                        Data = JsonConvert.DeserializeObject(
                            "{\"message\": \"<script>alert('An error occurred, please try again.');</script>\"}")
                    };
                }
            }

            // Update user profile image
            if (file != null &&  file.ContentLength > 0) 
            {
                // Get old image name, if any, to delete
                var oldImage = imageDir + userName + ".png";
                
                if (System.IO.File.Exists(oldImage))
                {
                    System.IO.File.Delete(oldImage);
                }
		
                var extension = Path.GetExtension(file.FileName).ToLower();
                var newFilename = Path.Combine(imageDir, userName);
                newFilename += extension;

                logger.Info("Saving new profile image: " + newFilename);

                file.SaveAs(newFilename);
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
            var msg = "Successfully changed values!\\\\nusername: {0}\\\\nReal Name: {1}\\\\nBlab Name: {2}";

            // TODO: Log user out if the username has changed.
            if (User.Identity.Name != userName)
            {
                InvalidateSession();
                return Content("{\"loggedout\":\"true\"}", "application/json");
            }

            // Don't forget to escape braces so they're not included in the string.Format
            var respTemplate = "{{\"values\": {{\"username\": \"{0}\", \"realName\": \"{1}\", \"blabName\": \"{2}\"}}, \"message\": \"<script>alert('"+ msg + "');</script>\"}}";
            return Content(string.Format(respTemplate, userName.ToLower(), realName, blabName), "application/json");
        }

        [HttpGet, ActionName("PasswordHint")]
        [AllowAnonymous]
        public ActionResult GetPasswordHint(string userName)
        {
            logger.Info("Entering password-hint with username: " + userName);
		
            if (string.IsNullOrEmpty(userName))
            {
                return Content("No username provided, please type in your username first");
            }

            try
            {
                using (var dbContext = new BlabberDB())
                {
                    var match = dbContext.Users.FirstOrDefault(x => x.UserName == userName);
                    if (match == null)
                    {
                        return Content("No password found for " + userName);
                    }

                    if (match.PasswordHint == null)
                    {
                        return Content("Username '" + userName + "' has no password hint!");
                    }

                    var formatString = "Username '" + userName + "' has password: {0}";
                    return Content(string.Format(formatString,
                        match.PasswordHint.Substring(0, 2) + new string('*', match.PasswordHint.Length - 2)));
                }
            }
            catch (Exception)
            {
                return Content("ERROR!");
            }
        }

        private bool UpdateUsername(string oldUsername, string newUsername)
        {
            // Enforce all lowercase usernames
            oldUsername = oldUsername.ToLower();
            newUsername = newUsername.ToLower();

            string[] sqlStrQueries =
            {
                "UPDATE users SET username=@newusername WHERE username=@oldusername",
                "UPDATE blabs SET blabber=@newusername WHERE blabber=@oldusername",
                "UPDATE comments SET blabber=@newusername WHERE blabber=@oldusername",
                "UPDATE listeners SET blabber=@newusername WHERE blabber=@oldusername",
                "UPDATE listeners SET listener=@newusername WHERE listener=@oldusername",
                "UPDATE users_history SET blabber=@newusername WHERE blabber=@oldusername"
            };

            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();

                foreach (var sql in sqlStrQueries)
                {
                    using (var update = connection.CreateCommand())
                    {
                        logger.Info("Preparing the Prepared Statement: " + sql);
                        update.CommandText = sql;
                        update.Parameters.Add(new SqlParameter {ParameterName = "@oldusername", Value = oldUsername});
                        update.Parameters.Add(new SqlParameter {ParameterName = "@newusername", Value = newUsername});
                        update.ExecuteNonQuery();
                    }
                }
            }

            var imageDir = HostingEnvironment.MapPath("~/Images/");
            var oldFilename = Path.Combine(imageDir, oldUsername) + ".png";
            var newFilename = Path.Combine(imageDir, newUsername) + ".png";

            if (System.IO.File.Exists(oldFilename))
            {
                System.IO.File.Move(oldFilename, newFilename);
            }

            return true;
        }

        private bool UsernameExists(string username)
        {
            username = username.ToLower();

            // Check is the username already exists
            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();

                var usernameCheck = connection.CreateCommand();

                usernameCheck.CommandText = "SELECT username FROM users WHERE username=?";
                var results = dbContext.Users.FirstOrDefault(x => x.UserName == username);

                return results != null;
            }
        }

        private void PopulateProfileViewModel(DbConnection connect, string username, ProfileViewModel viewModel)
        {
            string sqlMyProfile = "SELECT username, real_name, blab_name FROM users WHERE username = '" + username + "'";
            logger.Info(sqlMyProfile);

            using (var eventsCommand = connect.CreateCommand())
            {
                eventsCommand.CommandText = sqlMyProfile;
                using (var userProfile = eventsCommand.ExecuteReader())
                {
                    if (userProfile.Read())
                    {
                        viewModel.UserName = userProfile.GetString(0);
                        viewModel.RealName = userProfile.GetString(1);
                        viewModel.BlabName = userProfile.GetString(2);
                        viewModel.Image = GetProfileImageNameFromUsername(viewModel.UserName);
                    }
                }
            }

        }
        
        [CustomAuthorize]
        [HttpGet, ActionName("DownloadProfileImage")]
	    public FileResult DownloadProfileImage(string image)
	    {
		    logger.Info("Entering downloadImage");

	        var imagePath = Path.Combine(HostingEnvironment.MapPath("~/Images/"), image); 

		    logger.Info("Fetching profile image: " + imagePath);

	        return File(imagePath, System.Net.Mime.MediaTypeNames.Application.Octet);
	    }

        [HttpGet, ActionName("register")]
        public ActionResult GetRegister()
        {
            logger.Info("Entering GetRegister");

            return View(new RegisterViewModel());
        }
        
        [HttpPost, ActionName("register")]
        public ActionResult PostRegister (string username)
        {
            logger.Info("PostRegister processRegister");
            var registerViewModel = new RegisterViewModel();

            var sql = "SELECT count(*) FROM users WHERE username = '" + username.ToLower() + "'";
            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();
                var checkUsername = connection.CreateCommand();
                checkUsername.CommandText = sql;

                var numUsernames = checkUsername.ExecuteScalar() as int?;

                registerViewModel.UserName = username;

                if (numUsernames != 0)
                {
                    registerViewModel.Error = "Username '" + username + "' already exists!";
                    return View(registerViewModel);
                }

                return View("RegisterFinish", registerViewModel);
            }
        }

        private string GetProfileImageNameFromUsername(string viewModelUserName)
        {
            var imagePath = HostingEnvironment.MapPath("~/Images/");
            var image =  Directory.EnumerateFiles(imagePath).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == viewModelUserName);

            var filename = image == null ? "default_profile.png" : Path.GetFileName(image);
            
            return Url.Content("~/Images/" + filename);
        }

        private List<string> RetrieveMyEvents(DbConnection connect, string username)
        {
            var sqlMyEvents = "select event from users_history where blabber='" + 
                              username + "' ORDER BY eventid DESC; ";
            logger.Info(sqlMyEvents);
            
            var myEvents = new List<string>();
            using (var eventsCommand = connect.CreateCommand())
            {
                eventsCommand.CommandText = sqlMyEvents;
                using (var userHistoryResult = eventsCommand.ExecuteReader())
                {
                    while (userHistoryResult.Read())
                    {
                        myEvents.Add(userHistoryResult.GetString(0));
                    }
                }
            }

            return myEvents;
        }

        private static List<Blabber> RetrieveMyHecklers(DbConnection connect, string username)
        {
            var hecklers = new List<Blabber>();
            var sqlMyHecklers = "SELECT users.username, users.blab_name, users.created_at " +
                                "FROM users LEFT JOIN listeners ON users.username = listeners.listener " +
                                "WHERE listeners.blabber=@blabber AND listeners.status='Active'";

            using (var profile = connect.CreateCommand())
            {
                profile.CommandText = sqlMyHecklers;
                profile.Parameters.Add(new SqlParameter {ParameterName = "@blabber", Value = username});

                using (var myHecklersResults = profile.ExecuteReader())
                {
                    hecklers = new List<Blabber>();
                    while (myHecklersResults.Read())
                    {
                        var heckler = new Blabber
                        {
                            UserName = myHecklersResults.GetString(0),
                            BlabName = myHecklersResults.GetString(1),
                            CreatedDate = myHecklersResults.GetDateTime(2)
                        };
                        hecklers.Add(heckler);
                    }
                }
            }

            return hecklers;
        }

        [HttpGet, ActionName("RegisterFinish")]
        public ActionResult GetRegisterFinish()
        {
            logger.Info("Entering showRegisterFinish");

            return View();
        }

        [HttpPost, ActionName("RegisterFinish")]
        public ActionResult PostRegisterFinish(string userName, string password, string cpassword, string realName, string blabName)
        {
            if (password != cpassword)
            {
                logger.Info("Password and Confirm Password do not match");
                return View(new RegisterViewModel
                {
                    Error = "The Password and Confirm Password values do not match. Please try again.",
                    UserName = userName,
                    RealName = realName,
                    BlabName = blabName
                });
            }

            // Use the user class to get the hashed password.
            var user = new User(userName, password, DateTime.Now, null, blabName, realName);
            
            /* START BAD CODE */
            // Execute the query
            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();
                var createUser = connection.CreateCommand();
                var query = new StringBuilder();
                query.Append("insert into users (username, password, created_at, real_name, blab_name) values(");
                query.Append("'" + userName + "',");
                query.Append("'" + user.Password + "',");
                query.Append("CURRENT_TIMESTAMP,");
                query.Append("'" + realName + "',");
                query.Append("'" + blabName + "'");
                query.Append(");");

                createUser.CommandText = query.ToString();
                createUser.ExecuteNonQuery();
            }
            /* END BAD CODE */

            //EmailUser(userName);

            return RedirectToAction("Login", "Account", new LoginView {UserName = userName});
        }
    }
}