using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    [ContentProperty("CellTemplate")]
    public class GridColumn : QPControlBase
    {
        private UIElement _cellTemplate;
        public string Width { get; set; }

        public UIElement CellTemplate
        {
            get { return _cellTemplate; }
            set
            {
                _cellTemplate = value;
                if (value != null)
                {
                    _cellTemplate.Parent = this;
                }

            }
        }

        public bool UseForOrdering { get; set; }

        public bool UseForGroupping { get; set; }

        public Overflow Overflow { get; set; }
    }
}
