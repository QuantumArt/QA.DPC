using System.ComponentModel.DataAnnotations;

namespace QA.Core.Models.Configuration
{
    public enum DeletingMode
    {
		[Display(Name = "не удалять")]
        Keep=0,
        
		[Display(Name = "удалять")]
		Delete
    }
}
