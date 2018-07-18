using System.ComponentModel.DataAnnotations;

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
