using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{

    public interface IRegionService
    {
        /// <summary>
        /// Получение списка идентификаторов родителей по идентификатору региона
        /// </summary>
        /// <param name="id">Идентификатор региона</param>
        /// <returns></returns>
        List<int> GetParentsIds(int id);
    }
}
