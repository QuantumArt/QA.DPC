using Newtonsoft.Json.Linq;
using QA.Core.Models.Entities;
using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader
{
    internal class TmfProductDataSource : JsonProductDataSource
    {
        public TmfProductDataSource(IDictionary<string, JToken> article)
            : base(article)
        {
        }

        public override int GetArticleId()
        {
            var articleIdToken = GetJToken(nameof(Article.Id));

            if (articleIdToken == null)
            {
                return default;
            }

            return articleIdToken.Type == JTokenType.Integer
                ? articleIdToken.Value<int>()
                : default;
        }

        protected override JsonProductDataSource CreateDataSource(JToken token)
        {
            var tokenDict = (IDictionary<string, JToken>)token;

            return new TmfProductDataSource(
                new Dictionary<string, JToken>(tokenDict, StringComparer.OrdinalIgnoreCase));
        }
    }
}