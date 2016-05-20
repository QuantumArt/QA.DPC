using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
