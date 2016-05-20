using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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
