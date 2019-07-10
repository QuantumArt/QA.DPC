using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP.Services;
using QP.ConfigurationService.Models;

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
             var customerConfig = provider.GetCustomer(QP.Models.Service.Notification);
             if (!_contexts.TryGetValue(customerConfig.ConnectionString, out var result))
             {
                 if (customerConfig.DatabaseType == DatabaseType.Postgres)
                 {
                     var optionsBuilder = new DbContextOptionsBuilder<NpgSqlNotificationsModelDataContext>();
                     optionsBuilder.UseNpgsql(customerConfig.ConnectionString);
                     result = new NpgSqlNotificationsModelDataContext(optionsBuilder.Options);
                 }
                 else
                 {
                     var optionsBuilder = new DbContextOptionsBuilder<SqlServerNotificationsModelDataContext>();
                     optionsBuilder.UseSqlServer(customerConfig.ConnectionString);
                     result = new SqlServerNotificationsModelDataContext(optionsBuilder.Options);
                 }
                 _contexts[customerConfig.ConnectionString] = result;
             }

             return result;

        }

        public DbSet<Message> Messages{ get; set; }
    }
}