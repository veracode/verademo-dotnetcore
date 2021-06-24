using System.Data.Common;
using System.Reflection;

namespace Verademo.Commands
{
    public class BlabberCommandBase
    {
        protected readonly log4net.ILog logger;
        protected DbConnection connect;
        public BlabberCommandBase()
        {
            logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }
}