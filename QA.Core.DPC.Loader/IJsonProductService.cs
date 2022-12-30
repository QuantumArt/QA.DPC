using Newtonsoft.Json.Schema;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.Loader
{
    public interface IJsonProductService
    {
        string SerializeProduct(Article article, IArticleFilter filter, bool includeRegionTags = false);
        JSchema GetSchema(Content definition, bool forList = false, bool includeRegionTags = false);

        Article DeserializeProduct(string productJson, Content definition);
        string GetTypeName(string productJson);
    }
}