using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QA.ProductCatalog.HighloadFront.Models
{
    public class TaskItem
    {
        public string ChannelLanguage { get; set; }
        public string ChannelState { get; set; }
        public bool IsDefault { get; set; }
        public int? TaskId { get; set; }
        public State? TaskState { get; set; }
        public int? TaskProgress { get; set; }
        public DateTime? TaskStart { get; set; }
        public DateTime? TaskEnd { get; set; }
        public string TaskMessage { get; set; }

    }
}