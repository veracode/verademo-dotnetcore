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
            var listenerInsertQuery = "INSERT INTO listeners (blabber, listener, status) values (@blabber, @listener, 'Active');";
            logger.Info(listenerInsertQuery);

            using (var action = connect.CreateCommand())
            {

                action.CommandText = listenerInsertQuery;
                action.Parameters.Add(new SqlParameter {ParameterName = "@blabber", Value = blabberUsername});
                action.Parameters.Add(new SqlParameter {ParameterName = "@listener", Value = username});
                action.ExecuteNonQuery();
            }

            var blabNameQuery = "SELECT blab_name FROM users WHERE username = @username";
            logger.Info(blabNameQuery);
            string blabberName;

            using (var sqlStatement = connect.CreateCommand())
            {
                sqlStatement.CommandText = blabNameQuery;
                sqlStatement.Parameters.Add(new SqlParameter { ParameterName = "@username", Value = blabberUsername });

                using (var result = sqlStatement.ExecuteReader())
                {
                    result.Read();
                    blabberName = result.GetString((0));
                }
            }

            
            /* START BAD CODE */
            var listeningEvent = username + " started listening to " + blabberUsername + "(" + blabberName + ")";
            var eventQuery = "INSERT INTO users_history (blabber, event) VALUES ('" + username + "', '" + listeningEvent + "')";

            using (var sqlStatement = connect.CreateCommand())
            {
                logger.Info(eventQuery);
                sqlStatement.CommandText = eventQuery;
                sqlStatement.ExecuteNonQuery();
            }

            /* END BAD CODE */
        }
    }
}