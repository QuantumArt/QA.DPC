using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.ActionsRunnerModel.EntityModels
{
    public partial class TaskModel
    {

        public int ID { get; set; }
        public System.DateTime CreatedTime { get; set; }
        public Nullable<System.DateTime> LastStatusChangeTime { get; set; }
        public string Name { get; set; }
        public int StateID { get; set; }
        public string Data { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public Nullable<byte> Progress { get; set; }
        public string Message { get; set; }
        public bool IsCancellationRequested { get; set; }
        public bool IsCancelled { get; set; }
        public string DisplayName { get; set; }
        public Nullable<int> ScheduledFromTaskID { get; set; }
        public Nullable<int> ScheduleID { get; set; }
        public string ExclusiveCategory { get; set; }
        public string Config { get; set; }

    }
}
