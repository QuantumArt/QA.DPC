using System;
using System.Collections.Specialized;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TaskRunnerDelays
    {
        public TaskRunnerDelays()
        {
            MsToSleepIfNoDbAccess = 30000;
            MsToSleepIfNoTasks = 1000;
        }

        public int MsToSleepIfNoTasks { get; set; }

        public int MsToSleepIfNoDbAccess { get; set; }

    }
}