using QA.Workflow.Models;

namespace QA.Core.DPC.Workflow.Models
{
    public class ExtendedCamundaSettings : CamundaSettings
    {
        public TimeSpan TenantWatcherWaitTime { get; set; } = TimeSpan.FromMinutes(1);
    }
}
