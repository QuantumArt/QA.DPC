using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class NpgSqlTaskRunnerEntities : TaskRunnerEntities
    {
        public NpgSqlTaskRunnerEntities()
        {
        }

        public NpgSqlTaskRunnerEntities(DbContextOptions<NpgSqlTaskRunnerEntities> options)
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