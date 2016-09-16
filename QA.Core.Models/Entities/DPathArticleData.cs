using System.Collections.Generic;

namespace QA.Core.Models.Entities
{
    public class DPathArticleData
    {
        public string FieldName { get; set; }

        public List<DPathFilterData> FiltersData { get; set; }
    }
}
