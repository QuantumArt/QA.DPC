using QA.Core.Service.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Integration
{
    public interface IDpcProductService
    {
        ServiceResult<bool> HasProductChanged(int id, string data);

        ServiceResult<ProductInfo> Parse(string data);

        ServiceResult UpdateProduct(Product product, string data, string userName, int userId);

        ServiceResult DeleteProduct(int id);

    }
}
