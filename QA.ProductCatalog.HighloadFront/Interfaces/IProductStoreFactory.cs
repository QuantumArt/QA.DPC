using System;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStoreFactory
    {
        IProductStore GetProductStore(string language, string state);
        
        string GetProductStoreVersion(string language, string state);

        NotImplementedException ElasticVersionNotSupported(string serviceVersion);
    }
}
