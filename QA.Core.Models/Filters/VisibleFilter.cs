﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public class VisibleFilter : FilterBase
    {
        protected override bool OnMatch(Article item)
        {
            return item.Visible;
        }
    }
}
