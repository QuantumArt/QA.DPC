using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.DAL
{
    public class NotificationsModelDataContext : DbContext
    {

        private static Dictionary<string, NotificationsModelDataContext> _contexts 
            = new Dictionary<string, NotificationsModelDataContext>();
        public NotificationsModelDataContext()
        {

        }

        public NotificationsModelDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public static NotificationsModelDataContext GetOrCreate(IConnectionProvider provider)
        {
             var connectionString = provider.GetConnection(QP.Models.Service.Notification);
             if (!_contexts.TryGetValue(connectionString, out var result))
             {
                 if (provider.UsePostgres)
                 {
                     var optionsBuilder = new DbContextOptionsBuilder<NpgSqlNotificationsModelDataContext>();
                     optionsBuilder.UseNpgsql(connectionString);
                     result = new NpgSqlNotificationsModelDataContext(optionsBuilder.Options);
                 }
                 else
                 {
                     var optionsBuilder = new DbContextOptionsBuilder<SqlServerNotificationsModelDataContext>();
                     optionsBuilder.UseSqlServer(connectionString);
                     result = new SqlServerNotificationsModelDataContext(optionsBuilder.Options);
                 }
                 _contexts[connectionString] = result;
             }

             return result;

        }

        public DbSet<Message> Messages{ get; set; }
    }
}