using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.DAL
{
    public class SqlServerNotificationsModelDataContext: NotificationsModelDataContext
    {
        public SqlServerNotificationsModelDataContext()
        {
        }

        public SqlServerNotificationsModelDataContext(DbContextOptions<SqlServerNotificationsModelDataContext> options)
            : base(options)
        {
            
        }
    }
}