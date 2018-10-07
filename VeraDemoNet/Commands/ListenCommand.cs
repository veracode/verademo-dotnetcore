using System.Data.Common;
using System.Data.SqlClient;

namespace VeraDemoNet.Commands
{
    public class ListenCommand : BlabberCommandBase, IBlabberCommand
    {
        private readonly string username;

        public ListenCommand(DbConnection connect, string username)
        {
            this.connect = connect;
            this.username = username;
        }

        public void Execute(string blabberUsername)
        {
            var sqlQuery = "INSERT INTO listeners (blabber, listener, status) values (@blabber, @listener, 'Active');";
            logger.Info(sqlQuery);

            var action = connect.CreateCommand();
            action.CommandText = sqlQuery;
            action.Parameters.Add(new SqlParameter{ ParameterName = "@blabber", Value = blabberUsername});
            action.Parameters.Add(new SqlParameter{ ParameterName = "@listener", Value = username});
            action.ExecuteNonQuery();

            sqlQuery = "SELECT blab_name FROM users WHERE username = '" + blabberUsername + "'";
            var sqlStatement = connect.CreateCommand();
            sqlStatement.CommandText = sqlQuery;
            logger.Info(sqlQuery);
            var result = sqlStatement.ExecuteReader();
            result.NextResult();
            
            /* START BAD CODE */
            var listeningEvent = username + " started listening to " + blabberUsername + "(" + result.GetString(0) + ")";
            sqlQuery = "INSERT INTO users_history (blabber, event) VALUES (\"" + username + "\", \"" + listeningEvent + "\")";
            logger.Info(sqlQuery);
            sqlStatement.CommandText = sqlQuery;
            sqlStatement.ExecuteNonQuery();
            /* END BAD CODE */
        }
    }
}