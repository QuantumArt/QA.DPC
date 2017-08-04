using QA.Core.DPC.QP.Autopublish.Models;
using System.Collections.Generic;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public interface IAutopublishProvider
    {
        ProductItem[] Peek(string customerCode);
        ProductDescriptor GetProduct(ProductItem item, string format);
        void Dequeue(ProductItem item);
    }
}
