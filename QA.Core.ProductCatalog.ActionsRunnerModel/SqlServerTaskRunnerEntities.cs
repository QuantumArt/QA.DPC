using Microsoft.EntityFrameworkCore;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class SqlServerTaskRunnerEntities : TaskRunnerEntities
    {
        public SqlServerTaskRunnerEntities()
        {
        }

        public SqlServerTaskRunnerEntities(DbContextOptions<SqlServerTaskRunnerEntities> options)
            : base(options)
        {
        }

      
    }
}