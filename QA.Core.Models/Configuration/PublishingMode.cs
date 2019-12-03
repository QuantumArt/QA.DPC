using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.Core.Models.Configuration
{
    public enum PublishingMode
    {
        [Display(Name = "Publish", ResourceType = typeof(ControlStrings))]
        Publish = 0,
        
        [Display(Name = "DontPublish", ResourceType = typeof(ControlStrings))]
        SkipRecursive = 2
    }
}


