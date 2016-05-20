using System.Collections.Generic;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public interface IFieldService : IQPService
	{
		Field Read(int id);
		IEnumerable<Field> List(int contentId);
		IEnumerable<Field> ListRelated(int contentId);
	}
}