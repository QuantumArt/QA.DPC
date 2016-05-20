using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
using QA.Core.Extensions;
using QA.Core.Models.UI;
using QA.Core.DPC.UI.Controls;
using System.Windows.Markup;
using QA.Core.Models.Comparers;

namespace QA.Core.DPC.UI
{
    [ContentProperty("Columns")]
    public class GroupGridView : ItemListControl<GridRow>
    {
        #region Fields
        private readonly ItemList<GridColumn> _columns;

        private UIElement _groupingTemplate;

        private UIControl _rowTemplate;
        #endregion

        #region Properties
        public string Width { get; set; }

        public string GroupBy { get; set; }

        public string GroupingSource { get; set; }

        public bool Collapsible { get; set; }

        public GroupInitialFolding GroupInitialFolding { get; set; }

        public ItemList<GridColumn> Columns
        {
            get { return _columns; }
        }

        public bool Grouppable { get { return !string.IsNullOrEmpty(GroupBy); } }

        public UIElement GroupingTemplate
        {
            get
            {
                return _groupingTemplate;
            }
            set
            {
                _groupingTemplate = value;
                if (value != null)
                {
                    _groupingTemplate.Parent = this;
                }

            }
        }

        public UIControl RowTemplate
        {
            get
            {
                return _rowTemplate;
            }
            set
            {
                _rowTemplate = value;
                if (value != null)
                {
                    _rowTemplate.Parent = this;
                }

            }
        }

        #endregion

        public GroupGridView()
        {
            _columns = new ItemList<GridColumn>(this);
        }

        public IEnumerable<IGrouping<object, object>> GetGroups()
        {
            IEnumerable<object> items = Items;
            if (items != null && _columns != null)
            {
                if (Grouppable)
                {
                    if (OrderBy != null && OrderBy.Length > 0)
                    {
                        items = ApplyFilter(items);
                    }

                    return items.GroupBy(x => GetFilterValue(GroupBy, x), ModelObjectComparer.Default);
                }
            }

            return new List<IGrouping<object, object>>();

        }

        protected override GridRow GetTemplate()
        {
            return new GridRow(this, _columns);
        }
    }
}
