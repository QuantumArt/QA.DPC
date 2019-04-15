using System;
using System.Collections.Generic;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class Task
    {
        public Task()
        {
            SpawnedTasks = new HashSet<Task>();
        }
    
        public int ID { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? LastStatusChangeTime { get; set; }
        public string Name { get; set; }
        public int StateID { get; set; }
        public string Data { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public byte? Progress { get; set; }
        public string Message { get; set; }
        public bool IsCancellationRequested { get; set; }
        
        public bool IsCancelled { get; set; }
        public string DisplayName { get; set; }
        public int? ScheduledFromTaskID { get; set; }
        public int? ScheduleID { get; set; }
        public string ExclusiveCategory { get; set; }
        public string Config { get; set; }
        public byte[] BinData { get; set; }
    
        public Schedule Schedule { get; set; }
        public TaskState TaskState { get; set; }
        public ICollection<Task> SpawnedTasks { get; set; }
        public Task SheduledFromTask { get; set; }
    }
}
