using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Verademo.Models;
using Verademo.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Verademo.Controllers
{
    public class ResetController : AuthControllerBase
    {
        protected readonly log4net.ILog logger;
        private readonly IWebHostEnvironment _environment;
        private static readonly User[] _veraUsers =
        {
            Models.User.Create("admin", "admin", "Thats Mr Administrator to you.", true),
            Models.User.Create("john", "John", "John Smith"),
            Models.User.Create("paul", "Paul", "Paul Farrington"),
            Models.User.Create("chrisc", "Chris", "Chris Campbell"),
            Models.User.Create("laurie", "Laurie", "Laurie Mercer"),
            Models.User.Create("nabil", "Nabil", "Nabil Bousselham"),
            Models.User.Create("julian", "Julian", "Julian Totzek-Hallhuber"),
            Models.User.Create("joash", "Joash", "Joash Herbrink"),
            Models.User.Create("andrzej", "Andrzej", "Andrzej Szaryk"),
            Models.User.Create("april", "April", "April Sauer"),
            Models.User.Create("armando", "Armando", "Armando Bioc"),
            Models.User.Create("ben", "Ben", "Ben Stoll"),
            Models.User.Create("brian", "Brian", "Brian Pitta"),
            Models.User.Create("caitlin", "Caitlin", "Caitlin Johanson"),
            Models.User.Create("christraut", "Chris Trautwein", "Chris Trautwein"),
            Models.User.Create("christyson", "Chris Tyson", "Chris Tyson"),
            Models.User.Create("clint", "Clint", "Clint Pollock"),
            Models.User.Create("cody", "Cody", "Cody Bertram"),
            Models.User.Create("derek", "Derek", "Derek Chowaniec"),
            Models.User.Create("glenn", "Glenn", "Glenn Whittemore"),
            Models.User.Create("grant", "Grant", "Grant Robinson"),
            Models.User.Create("gregory", "Gregory", "Gregory Wolford"),
            Models.User.Create("jacob", "Jacob", "Jacob Martel"),
            Models.User.Create("jeremy", "Jeremy", "Jeremy Anderson"),
            Models.User.Create("johnny", "Johnny", "Johnny Wong"),
            Models.User.Create("kevin", "Kevin", "Kevin Rise"),
            Models.User.Create("scottrum", "Scott Rumrill", "Scott Rumrill"),
            Models.User.Create("scottsim", "Scott Simpson", "Scott Simpson")
        };

        public ResetController(IWebHostEnvironment environment)
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            _environment = environment;
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

                using (var dbContext = new ApplicationDbContext())
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
            var scriptPath = Path.Combine(_environment.ContentRootPath, "Resources/scripts/blab_schema.sql");

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

        private void AddUserData(ApplicationDbContext context)
        {
            logger.Info("Preparing the Statement for adding users");

            foreach (var user in _veraUsers)
            {
                logger.Info("Adding user " + user.UserName);
                context.Users.Add(user);
            }
            context.SaveChanges();
        }

        private string[] AddBlabs(DbConnection connect)
        {
            logger.Info("Reading blabs from file");

            var rand = new Random();

            var scriptPath = Path.Combine(_environment.ContentRootPath, "Resources/scripts/blabs.txt");
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
            
            var scriptPath = Path.Combine(_environment.ContentRootPath, "Resources/scripts/comments.txt");
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