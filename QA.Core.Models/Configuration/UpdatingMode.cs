using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.Core.Models.Configuration
{
    public enum UpdatingMode : byte
    {
        [Display(Name="Ignore", ResourceType = typeof(ControlStrings))]
        Ignore = 0,
        
        [Display(Name="UpdateOrCreate", ResourceType = typeof(ControlStrings))]
        Update
    }
}
