using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.TmForum
{
    internal class TmfProductDataSource : JsonProductDataSource
    {
        public TmfProductDataSource(IDictionary<string, JToken> article)
            : base(article)
        {
        }

        public override int GetArticleId()
        {
            JToken articleIdToken = GetJToken(nameof(Article.Id));

            if (articleIdToken is null || articleIdToken.Type != JTokenType.Integer)
                return default;

            return articleIdToken.Value<int>();
        }

        protected override JsonProductDataSource CreateDataSource(JToken token)
        {
            IDictionary<string, JToken> tokenDict = (IDictionary<string, JToken>)token;

            return new TmfProductDataSource(
                new Dictionary<string, JToken>(tokenDict, StringComparer.OrdinalIgnoreCase));
        }
    }
}
