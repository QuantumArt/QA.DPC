using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.Entities
{
    public interface IProductDataSource
    {
        int? GetInt(string fieldName);
        DateTime? GetDateTime(string fieldName);
        decimal? GetDecimal(string fieldName);
        IProductDataSource GetContainer(string fieldName);
        string GetString(string fieldName);
        IEnumerable<IProductDataSource> GetContainersCollection(string fieldName);
    }
}
