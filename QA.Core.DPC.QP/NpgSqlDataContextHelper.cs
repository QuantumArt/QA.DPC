using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.QP
{
    public class NpgSqlDataContextHelper
    {
        public static void NpgSqlDefaultOptions(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(ToSnakeCase(property.GetColumnBaseName()));

                foreach (var key in entity.GetKeys())
                    key.SetName(ToSnakeCase(key.GetName()));

                foreach (var key in entity.GetForeignKeys())
                    key.SetConstraintName(ToSnakeCase(key.GetConstraintName()));

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}