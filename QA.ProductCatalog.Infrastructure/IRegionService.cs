using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{

    public interface IRegionService
    {
        List<int> GetParentsIds(int id);
    }
}
