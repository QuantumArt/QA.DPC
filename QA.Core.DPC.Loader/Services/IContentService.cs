using System.Collections.Generic;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public interface IContentService : IQPService
	{
		Content Read(int id);
        bool Exists(int id);
        IEnumerable<Content> List(int siteId);
    }
}