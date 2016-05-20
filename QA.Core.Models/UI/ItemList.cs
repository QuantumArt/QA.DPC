using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public class ItemList : ItemList<UIElement>
    {
        public ItemList(UIElement elem)
            : base(elem)
        {

        }
    }
}
