using System.Collections.Generic;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class GridRow : UIElement
    {
        public ItemList<GridColumn> Columns { get; private set; }

        public GridRow(UIElement parent, IEnumerable<GridColumn> columns)
        {
            Parent = parent;
            Columns = new ItemList<GridColumn>(this, columns);
            if (Columns != null)
            {
                foreach (var item in Columns)
                {
                    if (item.CellTemplate == null)
                        continue;

                    item.CellTemplate.Parent = this;
                    item.Parent = this;
                }
            }
        }
    }
}
