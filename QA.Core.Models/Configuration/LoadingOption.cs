using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Configuration
{
    public class LoadingOption
    {
        public bool Load { get; set; }
        public CloningMode CloningMode { get; set; }
        public DeletingMode DeletingMode { get; set; }
    }
}
