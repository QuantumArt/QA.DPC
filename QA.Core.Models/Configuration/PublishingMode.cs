using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Configuration
{
    public enum PublishingMode
    {
		[Display(Name = "публиковать")]
        Publish = 0,
		[Display(Name = "не публиковать")]
        SkipRecursive = 2
    }
}
