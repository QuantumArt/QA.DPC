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

        public JsonProductDataSource(IDictionary<string, JToken> article)
        {
            _article = article;
        }

        public virtual int GetArticleId() => GetInt(nameof(Article.Id)) ?? default(int);

        public virtual DateTime GetModified() => default(DateTime);

        public int? GetInt(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToInt32(value) : (int?)null;
        }

        public DateTime? GetDateTime(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToDateTime(value) : (DateTime?)null;
        }

        public decimal? GetDecimal(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToDecimal(value) : (decimal?)null;
        }
        
        public string GetString(string fieldName)
        {
            return (string)_article[fieldName];
        }

        public virtual IProductDataSource GetContainer(string fieldName)
        {
            object value = _article[fieldName];

            return value == null ? null : new JsonProductDataSource((IDictionary<string, JToken>)value);
        }

        public virtual IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            object value = _article[fieldName];

            return value == null ? null : ((JArray)value)
                .Select(x => new JsonProductDataSource((IDictionary<string, JToken>)x));
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
            object value = _article[fieldName];

            return value == null ? null : new EditorJsonProductDataSource((IDictionary<string, JToken>)value);
        }
        
        public override IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            object value = _article[fieldName];

            return value == null ? null : ((JArray)value)
                .Select(x => new EditorJsonProductDataSource((IDictionary<string, JToken>)x));
        }

        public override IProductDataSource GetExtensionContainer(string fieldName, string extensionContentName)
        {
            // при десериализации для редактора используем объект [$"{fieldName}_Contents"][extensionContentName]
            return GetContainer(ArticleObject._Contents(fieldName)).GetContainer(extensionContentName);
        }
    }
}