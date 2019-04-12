using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.Front.DAL
{

    public class NpgSqlDpcModelDataContext : DpcModelDataContext

    {
        
        public NpgSqlDpcModelDataContext()
        {
        }

        public NpgSqlDpcModelDataContext(DbContextOptions<NpgSqlDpcModelDataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            foreach(var entity in modelBuilder.Model.GetEntityTypes()) 
            {
                entity.Relational().TableName = ToSnakeCase(entity.Relational().TableName);

                foreach(var property in entity.GetProperties()) 
                    property.Relational().ColumnName = ToSnakeCase(property.Name);

                foreach(var key in entity.GetKeys()) 
                    key.Relational().Name = ToSnakeCase(key.Relational().Name);

                foreach(var key in entity.GetForeignKeys())
                    key.Relational().Name = ToSnakeCase(key.Relational().Name);

                foreach(var index in entity.GetIndexes())
                    index.Relational().Name = ToSnakeCase(index.Relational().Name);
            }
        }
        
        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }
            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
        
        
    }
}