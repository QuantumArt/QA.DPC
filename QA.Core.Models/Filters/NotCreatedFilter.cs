using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public class NotCreatedFilter : FilterBase
    {
        protected override bool OnMatch(Article item)
        {
            return item.Id <= 0;
        }
    }
}