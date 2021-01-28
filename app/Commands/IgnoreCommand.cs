using System.Data.Common;
using System.Data.SqlClient;

namespace Verademo.Commands
{
    public class IgnoreCommand : BlabberCommandBase,IBlabberCommand
    {
        private readonly string username;

        public IgnoreCommand(DbConnection connect, string username)
        {
            this.connect = connect;
            this.username = username;
        }

        public void Execute(string blabberUsername) {
            var sqlQuery = "DELETE FROM listeners WHERE blabber=@blabber AND listener=@username";

            logger.Info(sqlQuery);

            var action = connect.CreateCommand();
            action.CommandText = sqlQuery;
            action.Parameters.Add(new SqlParameter{ParameterName = "@blabber", Value = blabberUsername});
            action.Parameters.Add(new SqlParameter{ParameterName = "@username", Value = username});
            action.ExecuteNonQuery();

            sqlQuery = "SELECT blab_name FROM users WHERE username = '" + blabberUsername + "'";
            
            var sqlStatement = connect.CreateCommand();
            sqlStatement.CommandText = sqlQuery;
            logger.Info(sqlQuery);
            var blabName = sqlStatement.ExecuteScalar();

            /* START BAD CODE */
            var ignoringEvent = username + " is now ignoring " + blabberUsername + "(" + blabName + ")";
            sqlQuery = "INSERT INTO users_history (blabber, event) VALUES (\"" + username + "\", \"" + ignoringEvent + "\")";

            sqlStatement.CommandText = sqlQuery;

            /* END BAD CODE */

            logger.Info(sqlQuery);
            sqlStatement.ExecuteNonQuery();
        }
    }
}