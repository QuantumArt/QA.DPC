using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader.Editor;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader
{
    /// <summary>
    /// ProductDataSource для формата редактора продуктов
    /// </summary>
    internal class EditorJsonProductDataSource : JsonProductDataSource
    {
        public EditorJsonProductDataSource(IDictionary<string, JToken> article)
            : base(article)
        {
        }

        public override int GetArticleId() => GetInt(ArticleObject._ServerId) ?? default(int);

        public override DateTime GetModified() => GetDateTime(ArticleObject._Modified) ?? default(DateTime);

        public override IProductDataSource GetContainer(string fieldName)
        {
            var token = GetJToken(fieldName);

            return token == null ? null : new EditorJsonProductDataSource((IDictionary<string, JToken>)token);
        }

        public override IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            var token = GetJToken(fieldName);

            return token == null ? null : ((JArray)token)
                .Select(x => new EditorJsonProductDataSource((IDictionary<string, JToken>)x));
        }

        public override IProductDataSource GetExtensionContainer(string fieldName, string extensionContentName)
        {
            // при десериализации для редактора используем объект [$"{fieldName}_Extension"][extensionContentName]
            return GetContainer(ArticleObject._Extension(fieldName)).GetContainer(extensionContentName);
        }
    }
}