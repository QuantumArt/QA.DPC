using System.Collections.Generic;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class Schedule
    {
        public Schedule()
        {
            Tasks = new HashSet<Task>();
        }
    
        public int ID { get; set; }
        public bool Enabled { get; set; }
        public bool AllowConcurrentTasks { get; set; }
        public string CronExpression { get; set; }
    
        public ICollection<Task> Tasks { get; set; }
    }
}
