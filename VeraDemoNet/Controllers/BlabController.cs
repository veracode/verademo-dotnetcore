using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using VeraDemoNet.Commands;
using VeraDemoNet.DataAccess;
using VeraDemoNet.Models;

namespace VeraDemoNet.Controllers
{
    public class BlabController : AuthControllerBase
    {
        protected readonly log4net.ILog logger;

        private string sqlBlabsByMe = 
            "SELECT b.content, b.timestamp, COUNT(c.blabid), b.blabid "  +
            "FROM blabs b LEFT JOIN comments c ON b.blabid = c.blabid " +
            "WHERE b.blabber = @username "+
            "GROUP BY b.content, b.timestamp, c.blabid, b.blabid "+
            "ORDER BY b.timestamp DESC;";

        private string sqlBlabsForMe =
            "SELECT u.username, u.blab_name, b.content, b.timestamp, COUNT(c.blabid), b.blabid " +
            "FROM blabs b INNER JOIN users u ON b.blabber = u.username " +
            "INNER JOIN listeners l ON b.blabber = l.blabber " +
            "LEFT JOIN comments c ON b.blabid = c.blabid " +
            "WHERE l.listener = @listener " +
            "GROUP BY c.blabid, u.username, u.blab_name, b.content, b.timestamp,  b.blabid " +
            "ORDER BY b.timestamp DESC OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

        private string sqlSearchBlabs =
            "SELECT b.blabber, b.content, b.timestamp " +
            "FROM blabs b " +
            "WHERE b.content LIKE '%{0}%' " + 
            "ORDER BY b.timestamp DESC";
        
        private string sqlBlabDetails = 
            "SELECT blabs.content, users.blab_name " +
            "FROM blabs INNER JOIN users ON blabs.blabber = users.username " + 
            "WHERE blabs.blabid = @blabId";

        private string sqlBlabComments = 
            "SELECT users.username, users.blab_name, comments.content, comments.timestamp " +
            "FROM comments INNER JOIN users ON comments.blabber = users.username " +
            "WHERE comments.blabid = @blabId ORDER BY comments.timestamp DESC";

        private string sqlAddBlab = 
            "INSERT INTO blabs (blabber, content, timestamp) values (@username, @blabcontents, @timestamp);";

        private string sqlAddComment = 
            "INSERT INTO comments (blabid, blabber, content, timestamp) values (@blabId, @blabber, @content, @timestamp)";

        private string sqlBlabbers =
            "SELECT users.username, users.blab_name, users.created_at, "+
                "MAX(case when listeners.listener=@username then 1 else 0 end) as subscribed, "+
                "SUM(case when listeners.listener <> @username then 1 else 0 end) as totallisteners, "+
                "SUM(case when listeners.status='Active' then 1 else 0 end) as totallistening "+
            "FROM users LEFT JOIN listeners ON users.username = listeners.blabber "+
            "WHERE users.username NOT IN ('admin', @username) "+
            "GROUP BY users.username,users.blab_name, users.created_at " +
            "ORDER BY {0}";

        public BlabController()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);    
        }

        [ActionName("SearchBlabs"), HttpGet]
        public ActionResult GetSearchBlabs()
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            return View("SearchBlabs", new SearchBlabsViewModel());
        }

        [ActionName("SearchBlabs"), HttpPost]
        public ActionResult PostSearchBlabs(string searchText)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var searchBlabslist = new List<BlabSearchResultViewModel>();
            using (var dbContext = new BlabberDB())
            {
                dbContext.Database.Connection.Open();
                var searchBlabs = dbContext.Database.Connection.CreateCommand();
                searchBlabs.CommandText = string.Format(sqlSearchBlabs, searchText);
                
                var searchBlabsResults = searchBlabs.ExecuteReader();
                while (searchBlabsResults.Read())
                {
                    var post = new BlabSearchResultViewModel
                    {
                        Blabber = searchBlabsResults.GetString(0),
                        Content = searchBlabsResults.GetString(1),
                        BlabDate = searchBlabsResults.GetDateTime(2),
                    };

                    searchBlabslist.Add(post);
                }
            }

            var model = new SearchBlabsViewModel
            {
                Blabs = searchBlabslist,
                SearchText = searchText
            };

            return View("SearchBlabs", model);
        }


        [ActionName("Feed"), HttpGet]
        public ActionResult GetFeed()
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var username = GetLoggedInUsername();

            // Find the Blabs that this user listens to
            var feedBlabs = new List<Blab>();
            using (var dbContext = new BlabberDB())
            { 
                dbContext.Database.Connection.Open();
                var listeningBlabs = dbContext.Database.Connection.CreateCommand();
                listeningBlabs.CommandText = string.Format(sqlBlabsForMe, 0, 10);
                listeningBlabs.Parameters.Add(new SqlParameter("@listener", username));

                var blabsForMeResults = listeningBlabs.ExecuteReader();
                while (blabsForMeResults.Read())
                {
                    var author = new Blabber
                    {
                        UserName = blabsForMeResults.GetString(0),
                        BlabName = blabsForMeResults.GetString(1)
                    };

                    var post = new Blab
                    {
                        Id = blabsForMeResults.GetInt32(5),
                        Content = blabsForMeResults.GetString(2),
                        PostDate = blabsForMeResults.GetDateTime(3),
                        CommentCount = blabsForMeResults.GetInt32(4),
                        Author = author
                    };

                    feedBlabs.Add(post);
                }
            }

            // Find Blabs by this user
            var myBlabs = new List<Blab>();
            using (var dbContext = new BlabberDB())
            {
                dbContext.Database.Connection.Open();
                var listeningBlabs = dbContext.Database.Connection.CreateCommand();
                listeningBlabs.CommandText = sqlBlabsByMe;
                listeningBlabs.Parameters.Add(new SqlParameter{ParameterName = "@username", Value = username});

                var blabsByMeResults = listeningBlabs.ExecuteReader();
                while (blabsByMeResults.Read())
                {
                    var post = new Blab
                    {
                        Id = blabsByMeResults.GetInt32(3),
                        Content = blabsByMeResults.GetString(0),
                        PostDate = blabsByMeResults.GetDateTime(1),
                        CommentCount = blabsByMeResults.GetInt32(2),
                    };

                    myBlabs.Add(post);
                }
            }

            return View(new FeedViewModel
                {
                    BlabsByMe = myBlabs,
                    BlabsByOthers = feedBlabs,
                    CurrentUser = username
                }
            );
        }

        [ActionName("Feed"), HttpPost]
        public ActionResult PostFeed(string blab)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var username = GetLoggedInUsername();
            
            using (var dbContext = new BlabberDB())
            {
                dbContext.Database.Connection.Open();
                dbContext.Database.ExecuteSqlCommand(sqlAddBlab, 
                    new SqlParameter{ParameterName = "@username", Value = username},
                    new SqlParameter{ParameterName = "@blabcontents", Value = blab},
                    new SqlParameter{ParameterName = "@timestamp", Value = DateTime.Now});
            }

            return RedirectToAction("Feed");
        }

        [HttpGet, ActionName("Blab")]
        public ActionResult GetBlab(int blabId)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var blabViewModel = CreateBlabViewModel(blabId);
            return View(blabViewModel);
        }

        [HttpGet, ActionName("Blabbers")]
        public ActionResult GetBlabbers(string sort)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            if (string.IsNullOrWhiteSpace(sort)) 
            {
                sort = "blab_name ASC";
            }
            var username = Session["username"] as string;
            
            var viewModel = PopulateBlabbersViewModel(sort, username);

            return View(viewModel);
        }

        private BlabbersViewModel PopulateBlabbersViewModel(string sort, string username)
        {
            var viewModel = new BlabbersViewModel();
            var blabbersList = new List<Blabber>();

            try
            {
                using (var dbContext = new BlabberDB())
                {
                    dbContext.Database.Connection.Open();
                    var blabbers = dbContext.Database.Connection.CreateCommand();
                    blabbers.CommandText = string.Format(sqlBlabbers, sort);
                    blabbers.Parameters.Add(new SqlParameter("@username", username));

                    var blabbersResults = blabbers.ExecuteReader();
                    while (blabbersResults.Read())
                    {
                        var blabber = new Blabber
                        {
                            UserName = blabbersResults.GetString(0),
                            BlabName = blabbersResults.GetString(1),
                            CreatedDate = blabbersResults.GetDateTime(2),
                            Subscribed = blabbersResults.GetInt32(3) == 1,
                            NumberListeners = blabbersResults.GetInt32(4),
                            NumberListening = blabbersResults.GetInt32(5)
                        };

                        blabbersList.Add(blabber);
                    }
                }

                viewModel.Blabbers = blabbersList;
            }
            catch (DbException ex)
            {
                viewModel.Error = ex.Message;
            }

            return viewModel;
        }

        [HttpPost, ActionName("Blabbers")]
        public ActionResult PostBlabbers(string blabberUsername, string command)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var username = GetLoggedInUsername();

            try
            {
                using (var dbContext = new BlabberDB())
                {
                    dbContext.Database.Connection.Open();

                    var commandType = Type.GetType("VeraDemoNet.Commands." + UpperCaseFirst(command) + "Command");

                    /* START BAD CODE */
                    var cmdObj = (IBlabberCommand) Activator.CreateInstance(commandType, dbContext.Database.Connection, username);
                    cmdObj.Execute(blabberUsername);
                    /* END BAD CODE */
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            var viewModel = PopulateBlabbersViewModel("blab_name ASC", username);

            return View(viewModel);
        }


        [HttpPost, ActionName("Blab")]
        public ActionResult PostBlab(int blabId, string comment)
        {
            if (IsUserLoggedIn() == false)
            {
                return RedirectToLogin(HttpContext.Request.RawUrl);
            }

            var username = GetLoggedInUsername();

            var error = "";

            using (var dbContext = new BlabberDB())
            {
                dbContext.Database.Connection.Open();
                var result= dbContext.Database.ExecuteSqlCommand(sqlAddComment, 
                    new SqlParameter{ParameterName = "@blabid", Value = blabId},
                    new SqlParameter{ParameterName = "@blabber", Value = username},
                    new SqlParameter{ParameterName = "@content", Value = comment},
                    new SqlParameter{ParameterName = "@timestamp", Value = DateTime.Now});

                if (result == 0)
                {
                    error = "Failed to add comment";
                }
            }

            var blabViewModel = CreateBlabViewModel(blabId);
            blabViewModel.Error = error;

            return View(blabViewModel);
        }

        [HttpGet]
        public string GetMoreBlabs(int start, int numResults)
        {
            var username = GetLoggedInUsername();

            var template = "<li><div><div class='commenterImage'>" +
                           "<img src='" + Url.Content("~/Images/") +"{0}.png'>" + 
                           "</div>" + 
                           "<div class='commentText'>" + 
                           "<p>{1}</p>" +
                           "<span class='date sub-text'>by {2} on {3}</span><br>" + 
                           "<span class='date sub-text'><a href=\"Blab/Blab?blabid={4}\">{5} Comments</a></span>" + 
                           "</div>" + 
                           "</div></li>";

            // Get the Database Connection
            var returnTemplate = new StringBuilder();

            using (var dbContext = new BlabberDB())
            { 
                dbContext.Database.Connection.Open();
                var listeningBlabs = dbContext.Database.Connection.CreateCommand();
                listeningBlabs.CommandText = string.Format(sqlBlabsForMe, start, numResults);
                listeningBlabs.Parameters.Add(new SqlParameter("@listener", username));

                var blabsForMeResults = listeningBlabs.ExecuteReader();
                while (blabsForMeResults.Read())
                {
                    var blab = new Blab {PostDate = blabsForMeResults.GetDateTime(3)};

                    returnTemplate.Append(string.Format(template, blabsForMeResults.GetString(0), // username
                                                        blabsForMeResults.GetString(2), // blab content
                                                        blabsForMeResults.GetString(1), // blab name
                                                        blab.PostDateString, // timestamp
                                                        blabsForMeResults.GetInt32(5), // blabID
                                                        blabsForMeResults.GetInt32(4) // comment count
                    ));
                }
            }

            return returnTemplate.ToString();
        }

        private BlabViewModel CreateBlabViewModel(int blabId)
        {
            var blabViewModel = new BlabViewModel {BlabId = blabId};

            using (var dbContext = new BlabberDB())
            {
                dbContext.Database.Connection.Open();
                var blabDetails = dbContext.Database.Connection.CreateCommand();
                blabDetails.CommandText = sqlBlabDetails;
                blabDetails.Parameters.Add(new SqlParameter("@blabId", blabId));

                var blabDetailsResults = blabDetails.ExecuteReader();


                // If there is a record...
                if (blabDetailsResults.Read())
                {
                    // Get the blab contents
                    blabViewModel.Content = blabDetailsResults.GetString(0);
                    blabViewModel.BlabName = blabDetailsResults.GetString(1);
                    blabDetailsResults.Close();

                    // Now lets get the comments...
                    var blabComments = dbContext.Database.Connection.CreateCommand();
                    blabComments.CommandText = sqlBlabComments;
                    blabComments.Parameters.Add(new SqlParameter("@blabId", blabId));

                    var blabCommentsResults = blabComments.ExecuteReader();

                    var comments = new List<Comment>();
                    while (blabCommentsResults.Read())
                    {
                        var author = new Blabber
                        {
                            UserName = blabCommentsResults.GetString(0),
                            BlabName = blabCommentsResults.GetString(1)
                        };

                        var comment = new Comment
                        {
                            Content = blabCommentsResults.GetString(2),
                            TimeStamp = blabCommentsResults.GetDateTime(3),
                            Author = author
                        };

                        comments.Add(comment);
                    }

                    blabViewModel.Comments = comments;
                }
            }

            return blabViewModel;
        }

        private static string UpperCaseFirst(string subject)
        {
            return subject[0].ToString().ToUpper() + subject.Substring(1);
        }
    }
}