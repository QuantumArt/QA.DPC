using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public class PublishedFilter : FilterBase
    {
        protected override bool OnMatch(Article item)
        {
            return item.IsPublished;
        }
    }
}
