using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class FieldServiceFake : IFieldService
	{
		public Field Read(int id)
		{
			return null;
		}

		public IEnumerable<Field> List(int contentId)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<Field> ListRelated(int contentId)
		{
			throw new System.NotImplementedException();
		}

		public QPConnectionScope CreateQpConnectionScope()
		{
			throw new System.NotImplementedException();
		}
	}
}
