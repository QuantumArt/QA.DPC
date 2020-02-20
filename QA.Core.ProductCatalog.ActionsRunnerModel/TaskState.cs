using System;
using System.Collections.Generic;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    
    public class TaskState
    {
        public TaskState()
        {
            this.Tasks = new HashSet<Task>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
    
        public ICollection<Task> Tasks { get; set; }
    }
}
