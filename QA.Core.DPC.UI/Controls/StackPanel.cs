using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class StackPanel: UIItemsControl
    {
        public bool IsHorizontal { get; set; }

        public bool UseInlineForChildBlocks { get; set; }

        public bool ShowBorder { get; set; }
    }
}
