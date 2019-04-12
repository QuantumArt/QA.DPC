using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.DAL
{
    public partial class NotificationsModelDataContext : DbContext
    {
        public NotificationsModelDataContext()
        {
        }

        public NotificationsModelDataContext(DbContextOptions<NotificationsModelDataContext> options)
            : base(options)
        {
        }

        public static NotificationsModelDataContext Create(IConnectionProvider provider)
        {
             var connectionString = provider.GetConnection(QP.Models.Service.Notification);  
             var optionsBuilder = new DbContextOptionsBuilder<NotificationsModelDataContext>();
             optionsBuilder.UseSqlServer(connectionString);
             return new NotificationsModelDataContext(optionsBuilder.Options);
        }

        public virtual DbSet<Message> Messages{ get; set; }
    }
}