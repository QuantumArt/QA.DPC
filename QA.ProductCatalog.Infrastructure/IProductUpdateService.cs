using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IProductUpdateService
	{
		void Update(Article product, ProductDefinition definition, bool isLive = false);
	}
}