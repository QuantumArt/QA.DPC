using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader.Editor;
using QA.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader
{
    internal class JsonProductDataSource : IProductDataSource
    {
        protected readonly IDictionary<string, JToken> _article;

        protected JToken GetJToken(string fieldName)
        {
            _ = _article.TryGetValue(fieldName, out var token);

            return token == null || token.Type == JTokenType.Null ? null : token;
        }

        public JsonProductDataSource(IDictionary<string, JToken> article)
        {
            _article = new Dictionary<string, JToken>(article, StringComparer.OrdinalIgnoreCase);
        }

        public virtual int GetArticleId() => GetInt(nameof(Article.Id)) ?? default(int);

        public virtual DateTime GetModified() => default(DateTime);

        public int? GetInt(string fieldName)
        {
            var value = GetJToken(fieldName);

            return value != null ? Convert.ToInt32(value) : (int?)null;
        }

        public DateTime? GetDateTime(string fieldName)
        {
            var value = GetJToken(fieldName);
            if (value == null) return null;
            var dt = Convert.ToDateTime(value);
            DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return dt.ToLocalTime();
        }

        public decimal? GetDecimal(string fieldName)
        {
            var value = GetJToken(fieldName);

            return value != null ? Convert.ToDecimal(value) : (decimal?)null;
        }

        public string GetString(string fieldName)
        {
            return (string)GetJToken(fieldName);
        }

        public virtual IProductDataSource GetContainer(string fieldName)
        {
            var token = GetJToken(fieldName);

            return token == null
                ? null
                : new JsonProductDataSource(
                    new Dictionary<string, JToken>((IDictionary<string, JToken>)token, StringComparer.OrdinalIgnoreCase));
        }

        public virtual IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            var token = GetJToken(fieldName);

            return token == null ? null : ((JArray)token)
                .Select(x => new JsonProductDataSource(
                    new Dictionary<string, JToken>((IDictionary<string, JToken>)x, StringComparer.OrdinalIgnoreCase)));
        }

        public virtual IProductDataSource GetExtensionContainer(string fieldName, string extensionContentName) => this;
    }

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