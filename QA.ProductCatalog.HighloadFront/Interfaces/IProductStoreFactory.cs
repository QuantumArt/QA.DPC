using System;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStoreFactory
    {
        Task<IProductStore> GetProductStore(string customerCode, string language, string state);

        Task<string> GetProductStoreVersion(string customerCode, string language, string state);

        NotImplementedException ElasticVersionNotSupported(string serviceVersion);
    }
}
