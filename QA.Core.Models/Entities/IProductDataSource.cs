using System;
using System.Collections.Generic;

namespace QA.Core.Models.Entities
{
    public interface IProductDataSource
    {
        int GetArticleId();

        DateTime GetModified();

        int? GetInt(string fieldName);

        DateTime? GetDateTime(string fieldName);

        decimal? GetDecimal(string fieldName);

        string GetString(string fieldName);

        IProductDataSource GetContainer(string fieldName);
        
        IEnumerable<IProductDataSource> GetContainersCollection(string fieldName);

        IProductDataSource GetExtensionContainer(string fieldName, string extensionContentName);
    }
}
