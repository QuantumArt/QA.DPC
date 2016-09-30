using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.Integration
{
    [ServiceContract]
    public interface IDpcService
    {
        [OperationContract]
        int[] GetAllProductId(int page, int pageSize);

		[OperationContract]
		int[] GetLastProductId(int page, int pageSize, DateTime date);

		[OperationContract]
        string GetProduct(int id);

        [OperationContract]
        DAL.Product GetProductInfo(int id);      
    }
}
