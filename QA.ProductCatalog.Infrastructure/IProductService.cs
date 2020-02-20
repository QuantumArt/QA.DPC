using System.Collections.Generic;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductService
    {
        /// <summary>
        /// Получение структурированного продукта на основе XML с маппингом данных
        /// </summary>
        /// <param name="id">Идентификатор продукта, по которому надо сделать мапинг</param>
        /// <returns></returns>
		Article GetProductById(int id, bool isLive = false, ProductDefinition productDefinition = null);
        Article[] GetProductsByIds(int contentId, int[] ids, bool isLive = false);
        Article[] GetProductsByIds(int[] ids, bool isLive = false);

        /// <summary>
        /// Загрузить список статей <paramref name="articleIds"/> по описанию продукта <paramref name="content"/>
        /// </summary>
        Article[] GetProductsByIds(Content content, int[] articleIds, bool isLive = false);

        /// <summary>
        /// Получение базовой информании о продуктах
        /// </summary>
        /// <param name="ids">Идентификаторы продуктов</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns></returns>
        Article[] GetSimpleProductsByIds(int[] ids, bool isLive = false);

        /// <summar
        /// Получение структуры данных на основе XML с мапингом данных из БД 
        /// </summary>
        /// <param name="productTypeId">Идентификатор типа продукта</param>
        /// <returns></returns>
        ProductDefinition GetProductDefinition(int productTypeId, bool isLive = false);

        ProductDefinition GetProductDefinition(int productTypeId, int contentId, bool isLive = false);

		Dictionary<string, object>[] GetProductsList(ServiceDefinition definition, long startRow, long pageSize, bool isLive);
    }
}
