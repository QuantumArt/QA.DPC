using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.Loader
{
    internal class JsonProductDataSource : IProductDataSource
    {
        private readonly IDictionary<string, JToken> _article;

        public JsonProductDataSource(IDictionary<string, JToken> article)
        {
            _article = article;
        }

        public int? GetInt(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToInt32(value) : (int?) null;
        }

        public DateTime? GetDateTime(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToDateTime(value) : (DateTime?) null;
        }

        public decimal? GetDecimal(string fieldName)
        {
            object value = _article[fieldName];

            return value != null ? Convert.ToDecimal(value) : (decimal?)null;
        }

        public IProductDataSource GetContainer(string fieldName)
        {
            object value = _article[fieldName];

            return value == null ? null : new JsonProductDataSource((IDictionary<string, JToken>) value);
        }

        public string GetString(string fieldName)
        {
            return (string)_article[fieldName];
        }

        public IEnumerable<IProductDataSource> GetContainersCollection(string fieldName)
        {
            object value = _article[fieldName];

            return value == null ? null : ((JArray) value).Select(x => new JsonProductDataSource((IDictionary<string, JToken>)x));
        }

       
    }
}
