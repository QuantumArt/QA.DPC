using System.ComponentModel.DataAnnotations;

namespace QA.Core.Models.Configuration
{
    public enum PreloadingMode
    {
        [Display(Name = "не загружать")]
        None = 0,
        [Display(Name = "загружать сразу")]
        Eager = 1,
        [Display(Name = "загружать отложенно")]
        Lazy = 2,
    }
}
