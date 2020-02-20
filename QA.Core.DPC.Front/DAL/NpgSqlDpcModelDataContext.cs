using Microsoft.EntityFrameworkCore;
using QA.Core.DPC.QP;

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
            base.OnModelCreating(modelBuilder);
            NpgSqlDataContextHelper.NpgSqlDefaultOptions(modelBuilder);
        }
        
    }
}