using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Mvc;
using VeraDemoNet.DataAccess;

namespace VeraDemoNet.Controllers
{
    public class ResetController : AuthControllerBase
    {
        protected readonly log4net.ILog logger;

        private static readonly User[] _veraUsers =
        {
            DataAccess.User.Create("admin", "admin", "Thats Mr Administrator to you.", true),
            DataAccess.User.Create("john", "John", "John Smith"),
            DataAccess.User.Create("paul", "Paul", "Paul Farrington"),
            DataAccess.User.Create("chrisc", "Chris", "Chris Campbell"),
            DataAccess.User.Create("laurie", "Laurie", "Laurie Mercer"),
            DataAccess.User.Create("nabil", "Nabil", "Nabil Bousselham"),
            DataAccess.User.Create("julian", "Julian", "Julian Totzek-Hallhuber"),
            DataAccess.User.Create("joash", "Joash", "Joash Herbrink"),
            DataAccess.User.Create("andrzej", "Andrzej", "Andrzej Szaryk"),
            DataAccess.User.Create("april", "April", "April Sauer"),
            DataAccess.User.Create("armando", "Armando", "Armando Bioc"),
            DataAccess.User.Create("ben", "Ben", "Ben Stoll"),
            DataAccess.User.Create("brian", "Brian", "Brian Pitta"),
            DataAccess.User.Create("caitlin", "Caitlin", "Caitlin Johanson"),
            DataAccess.User.Create("christraut", "Chris Trautwein", "Chris Trautwein"),
            DataAccess.User.Create("christyson", "Chris Tyson", "Chris Tyson"),
            DataAccess.User.Create("clint", "Clint", "Clint Pollock"),
            DataAccess.User.Create("cody", "Cody", "Cody Bertram"),
            DataAccess.User.Create("derek", "Derek", "Derek Chowaniec"),
            DataAccess.User.Create("glenn", "Glenn", "Glenn Whittemore"),
            DataAccess.User.Create("grant", "Grant", "Grant Robinson"),
            DataAccess.User.Create("gregory", "Gregory", "Gregory Wolford"),
            DataAccess.User.Create("jacob", "Jacob", "Jacob Martel"),
            DataAccess.User.Create("jeremy", "Jeremy", "Jeremy Anderson"),
            DataAccess.User.Create("johnny", "Johnny", "Johnny Wong"),
            DataAccess.User.Create("kevin", "Kevin", "Kevin Rise"),
            DataAccess.User.Create("scottrum", "Scott Rumrill", "Scott Rumrill"),
            DataAccess.User.Create("scottsim", "Scott Simpson", "Scott Simpson")
        };

        public ResetController()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [HttpGet]
        [ActionName("Reset")]
        public ActionResult GetReset()
        {
            logger.Info("Entering showReset");

            return View(new ResetViewModel());
        }

        [HttpPost]
        [ActionName("Reset")]
        public ActionResult PostReset(string confirm, string primary)
        {
            logger.Info("Entering processReset");

            var viewModel = new ResetViewModel();

            if (string.IsNullOrEmpty(confirm))
            {
                viewModel.Error = "You must confirm to proceed";
                return View(viewModel);
            }

            try
            {
                logger.Info("Getting Database connection");

                using (var dbContext = new BlabberDB())
                {
                    var connection = dbContext.Database.Connection;
                    connection.Open();

                    RecreateDatabaseSchema(connection);
                    AddUserData(dbContext);
                    AddListeners(connection);
                    var blabsList = AddBlabs(connection);
                    AddComments(connection, blabsList);
                    
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                viewModel.Error = ex.Message;
            }

            return View(viewModel);
        }

        private void RecreateDatabaseSchema(DbConnection connect)
        {
            var scriptPath = HostingEnvironment.MapPath("~/Resources/scripts/blab_schema.sql");

            var script = System.IO.File.ReadAllText(scriptPath);

            // split script on GO command
            IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var commandString in commandStrings)
            {
                if (commandString.Trim() == "") continue;

                using (var command = connect.CreateCommand())
                {
                    command.CommandText = commandString;
                    command.ExecuteNonQuery();
                }
            }     
        }

        private void AddListeners(DbConnection connect)
        {
            var rand = new Random();

            // Add the listeners        
            logger.Info("Preparing the Statement for adding listeners");

            using (var tr = connect.BeginTransaction())
            {
                for (var i = 1; i < _veraUsers.Length; i++)
                {
                    var listenersStatement = connect.CreateCommand();
                    listenersStatement.Transaction = tr;
                    listenersStatement.CommandText =
                        "INSERT INTO listeners (blabber, listener, status) values (@blabber, @listener, 'Active');";

                    for (var j = 1; j < _veraUsers.Length; j++)
                    {
                        if (Convert.ToBoolean(rand.Next(0, 1)) && i != j)
                        {
                            var blabber = _veraUsers[i].UserName;
                            var listener = _veraUsers[j].UserName;

                            logger.Info("Adding " + listener + " as a listener of " + blabber);

                            listenersStatement.Parameters.Add(blabber);
                            listenersStatement.Parameters.Add(listener);

                            listenersStatement.ExecuteNonQuery();
                        }
                    }
                }

                tr.Commit();
            }

        }

        private void AddUserData(BlabberDB context)
        {
            logger.Info("Preparing the Statement for adding users");

            foreach (var user in _veraUsers)
            {
                logger.Info("Adding user " + user.UserName);
                user.Password = Md5Hash(user.Password);
                context.Users.Add(user);
            }
            context.SaveChanges();
        }

        private string[] AddBlabs(DbConnection connect)
        {
            logger.Info("Reading blabs from file");

            var rand = new Random();

            var scriptPath = HostingEnvironment.MapPath("~/Resources/scripts/blabs.txt");
            var blabsContent = System.IO.File.ReadAllLines(scriptPath);

            // Add the blabs
            logger.Info("Preparing the Statement for adding blabs");

            using (var tr = connect.BeginTransaction())
            {
                foreach (var blabContent in blabsContent)
                {
                    var blabsStatement = connect.CreateCommand();
                    blabsStatement.CommandText =
                        "INSERT INTO blabs (blabber, content, timestamp) values (@blabber, @content, @timestamp);";
                    blabsStatement.Transaction = tr;

                    // Get the array offset for a random user, except admin who's offset 0.
                    var randomUserOffset = rand.Next(_veraUsers.Length - 2) + 1;

                    // get the number of seconds until some time in the last 30 days.
                    long vary = rand.Next(30 * 24 * 3600);

                    var username = _veraUsers[randomUserOffset].UserName;
                    logger.Info("Adding a blab for " + username);

                    blabsStatement.Parameters.Add(new SqlParameter {ParameterName = "@blabber", Value = username});
                    blabsStatement.Parameters.Add(new SqlParameter {ParameterName = "@content", Value = blabContent});
                    blabsStatement.Parameters.Add(new SqlParameter {ParameterName = "@timestamp", Value = DateTime.Now.AddSeconds(vary * -1000)});

                    blabsStatement.ExecuteNonQuery();
                }

                tr.Commit();
            }

            return blabsContent;
        }

        private void AddComments(DbConnection connect, string[] blabsContent)
        {
            logger.Info("Reading comments from file");
            
            var rand = new Random();
            var scriptPath = HostingEnvironment.MapPath("~/Resources/scripts/comments.txt");
            var commentsContent = System.IO.File.ReadAllLines(scriptPath);

            // Add the comments
            logger.Info("Preparing the Statement for adding comments");

            using (var tr = connect.BeginTransaction())
            {
                for (var i = 1; i <= blabsContent.Length; i++)
                {
                    // Add a random number of comment
                    var count = rand.Next(6); // (between 0 and 6)

                    for (var j = 0; j < count; j++)
                    {
                        var commentsStatement = connect.CreateCommand();
                        commentsStatement.CommandText =
                            "INSERT INTO comments (blabid, blabber, content, timestamp) values (@blabid, @blabber, @content, @timestamp);";
                        commentsStatement.Transaction = tr;

                        // Get the array offset for a random user, except admin who's offset 0.
                        var randomUserOffset = rand.Next(_veraUsers.Length - 2) + 1;
                        var username = _veraUsers[randomUserOffset].UserName;

                        // Pick a random comment to add
                        var commentNum = rand.Next(commentsContent.Length);
                        var comment = commentsContent[commentNum];

                        // get the number of seconds until some time in the last 30 days.
                        long vary = rand.Next(30 * 24 * 3600);

                        logger.Info("Adding a comment from " + username + " on blab ID " + Convert.ToString(i));
                        commentsStatement.Parameters.Add(new SqlParameter {ParameterName = "@blabid", Value = i}  );
                        commentsStatement.Parameters.Add(new SqlParameter {ParameterName = "@blabber", Value = username}  );
                        commentsStatement.Parameters.Add(new SqlParameter {ParameterName = "@content", Value = comment}  );
                        commentsStatement.Parameters.Add(new SqlParameter {ParameterName = "@timestamp", Value = DateTime.Now.AddSeconds(vary * -1000)}  );

                        commentsStatement.ExecuteNonQuery();
                    }
                }

                tr.Commit();
            }
        }
    }
}