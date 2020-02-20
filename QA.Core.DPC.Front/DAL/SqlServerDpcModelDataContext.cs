
using Microsoft.EntityFrameworkCore;

namespace QA.Core.DPC.Front.DAL
{
    public class SqlServerDpcModelDataContext : DpcModelDataContext

    {
        public SqlServerDpcModelDataContext()
        {
        }

        public SqlServerDpcModelDataContext(DbContextOptions<SqlServerDpcModelDataContext> options)
            : base(options)
        {
        }

    }
}