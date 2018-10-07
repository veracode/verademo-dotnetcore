using System.Data.Common;

namespace VeraDemoNet.Commands
{
    public class RemoveAccountCommand : BlabberCommandBase,IBlabberCommand
    {
        public RemoveAccountCommand(DbConnection connect, string username) {
            this.connect = connect;
        }

        public void Execute(string blabberUsername)
        {
            var sqlQuery = "DELETE FROM listeners WHERE blabber=? OR listener=?;";
            logger.Info(sqlQuery);
            DbCommand action;
            action = connect.CreateCommand();
            action.CommandText = sqlQuery;
		
            action.Parameters.Add(blabberUsername);
            action.Parameters.Add(blabberUsername);
            action.ExecuteNonQuery();

            sqlQuery = "SELECT blab_name FROM users WHERE username = '" + blabberUsername +"'";
            var sqlStatement = connect.CreateCommand();
            sqlStatement.CommandText = sqlQuery;
            logger.Info(sqlQuery);
            var result = sqlStatement.ExecuteReader();
            result.NextResult();
		
            /* START BAD CODE */
            var removeEvent = "Removed account for blabber " + result.GetString(0);
            sqlQuery = "INSERT INTO users_history (blabber, event) VALUES ('" + blabberUsername + "', '" + removeEvent + "')";
            logger.Info(sqlQuery);
            sqlStatement.CommandText = sqlQuery;
            sqlStatement.ExecuteNonQuery();
		
            sqlQuery = "DELETE FROM users WHERE username = '" + blabberUsername + "'";
            logger.Info(sqlQuery);
            sqlStatement.CommandText = sqlQuery;
            sqlStatement.ExecuteNonQuery();
            /* END BAD CODE */
        }
    }
}