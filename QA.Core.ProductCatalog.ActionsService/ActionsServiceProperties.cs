using QA.Core.ProductCatalog.ActionsRunner;

namespace QA.Core.ProductCatalog.ActionsService
{
    public class ActionsServiceProperties
    {
        public ActionsServiceProperties()
        {
            EnableScheduleProcess = true;
            NumberOfThreads = 4;
            Delays = new TaskRunnerDelays();
        }
        public bool EnableScheduleProcess { get; set; }
        
        public int NumberOfThreads { get; set; }
        
        public TaskRunnerDelays Delays { get; set; }
        
        public string Name { get; set; }
        
    }
}