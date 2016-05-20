using System.Data.Entity;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public partial class TaskRunnerEntities : DbContext
    {
        public TaskRunnerEntities(string nameOrConnectionString)
             : base(nameOrConnectionString) 
        {
        }
    }
}
