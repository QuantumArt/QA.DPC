using System.ComponentModel.DataAnnotations;

namespace QA.Core.Models.Configuration
{
    public enum UpdatingMode : byte
    {
        [Display(Name = "игнорировать")]
        Ignore =0,
        [Display(Name = "обновлять/создавать")]
        Update
    }
}
