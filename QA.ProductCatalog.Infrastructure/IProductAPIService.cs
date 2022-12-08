using Newtonsoft.Json.Linq;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API.Models;
using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductAPIService
    {
        Dictionary<string, object>[] GetProductsList(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue);
        int[] SearchProducts(string slug, string version, string query, bool isLive = false);
        int[] ExtendedSearchProducts(string slug, string version, JToken query, bool isLive = false);
        Article GetProduct(string slug, string version, int id, bool isLive = false, bool includeRelevanceInfo = false);
        void UpdateProduct(string slug, string version, Article product, bool isLive = false, bool createVersions = false);
        void CustomAction(string actionName, int id, int contentId = default(int));
        void CustomAction(string actionName, int[] ids, int contentId = default(int));
        void CustomAction(string actionName, int id, Dictionary<string, string> parameters, int contentId = default(int));
        void CustomAction(string actionName, int[] ids, Dictionary<string, string> parameters, int contentId = default(int));
        ServiceDefinition GetProductDefinition(string slug, string version, bool forList = false);
        RelevanceInfo GetRelevance(int id, bool isLive = false);
        int? CreateProduct(string slug, string version, Article product, bool isLive = false, bool createVersions = false);
        void DeleteProduct(string slug, string version, int id);
    }
}
