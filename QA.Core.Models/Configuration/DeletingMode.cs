using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.Core.Models.Configuration
{
    public enum DeletingMode
    {
	    [Display(Name="DontRemove", ResourceType = typeof(ControlStrings))]
        Keep = 0,
        
        [Display(Name="Remove", ResourceType = typeof(ControlStrings))]
		Delete
    }
}
