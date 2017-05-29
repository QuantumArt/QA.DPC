using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.QP.Autopublish.Models
{
    public class ProductItem
    {
        public string CustomerCode { get; set; }
        public int ProductId { get; set; }
        public int DefinitionId { get; set; }
        public string Slug { get; set; }
        public string Version { get; set; }
    }
}
