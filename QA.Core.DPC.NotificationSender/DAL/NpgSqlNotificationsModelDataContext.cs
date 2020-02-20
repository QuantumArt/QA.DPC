using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP;

namespace QA.Core.DPC.DAL
{
    public class NpgSqlNotificationsModelDataContext: NotificationsModelDataContext
    {
        public NpgSqlNotificationsModelDataContext()
        {
        }

        public NpgSqlNotificationsModelDataContext(DbContextOptions<NpgSqlNotificationsModelDataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            NpgSqlDataContextHelper.NpgSqlDefaultOptions(modelBuilder);
        }
    }
}