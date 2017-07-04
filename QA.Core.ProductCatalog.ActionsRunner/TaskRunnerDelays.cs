using System;
using System.Collections.Specialized;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TaskRunnerDelays
    {
        public TaskRunnerDelays()
        {
            MsToSleepIfNoDbAccess = 30000;
            MsToSleepIfNoTasks = 100;
        }

        public int MsToSleepIfNoTasks { get; set; }

        public int MsToSleepIfNoDbAccess { get; set; }

        public TaskRunnerDelays(NameValueCollection appSettings)
        {
            if (!string.IsNullOrEmpty(appSettings["MillisecondsToSleepIfNoTasks"]))
            {
                MsToSleepIfNoTasks = Convert.ToInt32(appSettings["MillisecondsToSleepIfNoTasks"]);
            }
            if (!string.IsNullOrEmpty(appSettings["MillisecondsToSleepIfNotAccessDB"]))
            {
                MsToSleepIfNoDbAccess = Convert.ToInt32(appSettings["MillisecondsToSleepIfNotAccessDB"]);
            }

        }


    }
}