using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public virtual DbSet<Message> Messages{ get; set; }
    }
}