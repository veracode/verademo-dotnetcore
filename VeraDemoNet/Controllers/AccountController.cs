using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;
using VeraDemoNet.DataAccess;
using VeraDemoNet.Models;

namespace VeraDemoNet.Controllers  
{  
    // https://www.c-sharpcorner.com/article/custom-authentication-with-asp-net-mvc/
    public class AccountController : AuthControllerBase
    {
        protected readonly log4net.ILog logger;

        private const string COOKIE_NAME = "UserDetails";

        public AccountController()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);    
        }

        [HttpGet, ActionName("Login")]
        public ActionResult GetLogin(string ReturnUrl = "")
        {
            logger.Info("Login page visited: " + ReturnUrl);

            if (IsUserLoggedIn())
            {
                return GetLogOut();
            }


            var userDetailsCookie = Request.Cookies[COOKIE_NAME];

            if (userDetailsCookie == null || userDetailsCookie.Value.Length == 0)
            {
                logger.Info("No user cookie");
                Session["username"] = "";

                ViewBag.ReturnUrl = ReturnUrl;
                return View();
            }

            logger.Info("User details were remembered");
            var unencodedUserDetails = Convert.FromBase64String(userDetailsCookie.Value);

            CustomSerializeModel deserializedUser;

            using (MemoryStream memoryStream = new MemoryStream(unencodedUserDetails))
            {
                var binaryFormatter = new BinaryFormatter();

                // set memory stream position to starting point
                memoryStream.Position = 0;

                // Deserializes a stream into an object graph and return as a object.
                /* START BAD CODE */
                deserializedUser = binaryFormatter.Deserialize(memoryStream) as CustomSerializeModel;
                /* END BAD CODE */
                logger.Info("User details were retrieved for user: " + deserializedUser.UserName);
            }

            Session["username"] = deserializedUser.UserName;

            //if (Url.IsLocalUrl(ReturnUrl))  
            if (string.IsNullOrEmpty(ReturnUrl))
            {
                return RedirectToAction("Feed", "Blab");
            }

            /* START BAD CODE */
            return Redirect(ReturnUrl);
            /* END BAD CODE */
        }

        [HttpPost, ActionName("Login")]
        public ActionResult PostLogin(LoginView loginViewModel, string ReturnUrl = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userDetails = LoginUser(loginViewModel.UserName, loginViewModel.Password);

                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "Application";
                        eventLog.WriteEntry("Entering PostLogin with target " + ReturnUrl + " and username " + loginViewModel.UserName, EventLogEntryType.Information, 101, 1);
                    }

                    if (userDetails == null)
                    {
                        ModelState.AddModelError("CustomError", "Something Wrong : UserName or Password invalid ^_^ ");
                        return View(loginViewModel);
                    }

                    if (loginViewModel.RememberLogin)
                    {
                        var userModel = new CustomSerializeModel()
                        {
                            UserName = userDetails.UserName,
                            BlabName = userDetails.BlabName,
                            RealName = userDetails.RealName
                        };

                        using (var userModelStream = new MemoryStream())
                        {
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(userModelStream, userModel);
                            var faCookie =
                                new HttpCookie(COOKIE_NAME, Convert.ToBase64String(userModelStream.GetBuffer()))
                                {
                                    Expires = DateTime.Now.AddDays(30)
                                };
                            Response.Cookies.Add(faCookie);
                        }
                    }

                    if (string.IsNullOrEmpty(ReturnUrl))
                    {
                        return RedirectToAction("Feed", "Blab");
                    }

                    /* START BAD CODE */
                    return Redirect(ReturnUrl);
                    /* END BAD CODE */
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", ex.Message);
            }

            return View(loginViewModel);

        }

        [HttpGet, ActionName("Logout")]
        public ActionResult GetLogOut()
        {
            var cookie = new HttpCookie("UserDetails", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };

            Response.Cookies.Add(cookie);

            LogoutUser();
            
            return Redirect(Url.Action("Login", "Account"));
        }

        [HttpGet, ActionName("Profile")]
        public ActionResult GetProfile()
        {
            logger.Info("Entering GetProfile");

            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var viewModel = new ProfileViewModel();

            var username = GetLoggedInUsername();

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

        [HttpPost, ActionName("Profile")]
        public ActionResult PostProfile(string realName, string blabName, string userName, HttpPostedFileBase file)
        {
            logger.Info("Entering PostProfile");

            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var oldUsername = GetLoggedInUsername();
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

                Session["username"] = userName;
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

            /* START BAD CODE */

            // Don't forget to escape braces so they're not included in the string.Format
            var respTemplate = "{{\"values\": {{\"username\": \"{0}\", \"realName\": \"{1}\", \"blabName\": \"{2}\"}}, \"message\": \"<script>alert('"+ msg + "');</script>\"}}";

            // JSON doesn't like single backslashes so escape them?
            return Content(string.Format(respTemplate, userName.ToLower().Replace("\\", "\\\\"), realName.Replace("\\", "\\\\"), blabName.Replace("\\", "\\\\")), "application/json");

            /* END BAD CODE */
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
                    return Content(string.Format(formatString, match.PasswordHint.Substring(0, 2) + new string('*', match.PasswordHint.Length - 2)));
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
            string sqlMyProfile = "SELECT username, real_name, blab_name, is_admin FROM users WHERE username = '" + username + "'";
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
                        viewModel.IsAdmin = userProfile.GetBoolean(3);
                        viewModel.Image = GetProfileImageNameFromUsername(viewModel.UserName);
                    }
                }
            }
        }
        
        [HttpGet, ActionName("DownloadProfileImage")]
	    public ActionResult DownloadProfileImage(string image)
	    {
		    logger.Info("Entering downloadImage");

	        if (IsUserLoggedIn() == false)
	        {
	            return RedirectToLogin(HttpContext.Request.RawUrl);
	        }

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

            Session["username"] = username;

            var sql = "SELECT count(*) FROM users WHERE username = @username";
            using (var dbContext = new BlabberDB())
            {
                var connection = dbContext.Database.Connection;
                connection.Open();
                var checkUsername = connection.CreateCommand();
                checkUsername.CommandText = sql;
                checkUsername.Parameters.Add(new SqlParameter {ParameterName = "@username", Value = username.ToLower()});

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
            // START BAD CODE
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

            // END BAD CODE

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
        public ActionResult PostRegisterFinish(User user, string cpassword)
        {
            if (user.Password != cpassword)
            {
                logger.Info("Password and Confirm Password do not match");
                return View(new RegisterViewModel
                {
                    Error = "The Password and Confirm Password values do not match. Please try again.",
                    UserName = user.UserName,
                    RealName = user.RealName,
                    BlabName = user.BlabName,
                });
            }

            // Use the user class to get the hashed password.
            user.Password = Md5Hash(user.Password);
            user.CreatedAt = DateTime.Now;
            
            using (var dbContext = new BlabberDB())
            {
                var connect = dbContext.Database.Connection;
                connect.Open();

                var query = "insert into users (username, password, created_at, real_name, blab_name) values(@username, @password, SYSDATETIME(), @realname, @blabname)";
                
                using (var update = connect.CreateCommand())
                {
                    logger.Info("Preparing the Prepared Statement: " + query);
                    update.CommandText = query;
                    update.Parameters.Add(new SqlParameter { ParameterName = "@username", Value = user.UserName });
                    update.Parameters.Add(new SqlParameter { ParameterName = "@password", Value = user.Password });
                    update.Parameters.Add(new SqlParameter { ParameterName = "@realname", Value = user.RealName });
                    update.Parameters.Add(new SqlParameter { ParameterName = "@blabname", Value = user.BlabName });

                    update.ExecuteNonQuery();
                }
            }

            var imageDir = HostingEnvironment.MapPath("~/Images/");
            try
            {
                System.IO.File.Copy(Path.Combine(imageDir, "default_profile.png"), Path.Combine(imageDir, user.UserName) + ".png");
            }
            catch (Exception ex)
            {

            }


            //EmailUser(userName);

            return RedirectToAction("Login", "Account", new LoginView {UserName = user.UserName});
        }
    }
}