﻿using Newtonsoft.Json.Linq;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader
{
    public class JsonProductDataSource : IProductDataSource
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
                : CreateDataSource(token);
        }

        public virtual IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            var token = GetJToken(fieldName);

            return token == null ? null : ((JArray)token)
                .Select(CreateDataSource);
        }

        protected virtual JsonProductDataSource CreateDataSource(JToken token)
        {
            // TODO: Roll back to using register dependent comparison
            var tokenDict = (IDictionary<string, JToken>)token;

            return new JsonProductDataSource(
                new Dictionary<string, JToken>(tokenDict, StringComparer.OrdinalIgnoreCase));
        }

        public virtual IProductDataSource GetExtensionContainer(string fieldName, string extensionContentName) => this;

        public decimal? GetBoolAsDecimal(string fieldName)
        {
            JToken token = GetJToken(fieldName);

            if (token is null)
            {
                return null;
            }

            token = Convert.ToBoolean(token);

            return Convert.ToDecimal(token);
        }
    }
}