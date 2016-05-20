using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QA.ProductCatalog.RegionSync.Core.Services
{
	public interface IRegionProvider<TModel>
		where TModel : class
	{
		TModel[] GetRegions(Dictionary<string, string> fieldMap);
		TModel[] GetTopRegions<TProperty>(Dictionary<string, string> fieldMap, Expression<Func<TModel, TProperty>> parentFieldSelector);
		void DeleteRegions(IEnumerable<int> ids);
		void UpdateRegions(IEnumerable<TModel> regions, Dictionary<string, string> fieldMap);
	}
}
