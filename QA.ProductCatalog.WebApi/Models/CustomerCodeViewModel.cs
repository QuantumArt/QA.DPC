using QA.Core.DPC.QP.Models;
using System;

namespace QA.ProductCatalog.WebApi.Models
{
    [Serializable]
    public class CustomerCodeViewModel
    {
        public string CustomerCode { get; set; }
        public string State { get; set; }
    }
}
