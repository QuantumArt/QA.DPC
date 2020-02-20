using System.Collections;
using System.Collections.Generic;
using QA.Core.DPC.UI;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class GetRowsModel
    {
        public GroupGridView GridView { get; set; }
        
        public IEnumerable<object> Items { get; set; }
        
        public bool Inner { get; set; }
        
        
    }
}