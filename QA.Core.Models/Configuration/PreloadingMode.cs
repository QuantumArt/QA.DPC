using System;
using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.Core.Models.Configuration
{
    public enum PreloadingMode
    {
        [Display(Name="IgnoreLoading", ResourceType = typeof(ControlStrings))]
        None = 0,

        [Display(Name="EagerLoading", ResourceType = typeof(ControlStrings))]
        Eager = 1,

        [Display(Name="LazyLoading", ResourceType = typeof(ControlStrings))]
        Lazy = 2,
    }
}

