using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Configuration;

namespace QA.Core.DPC.Loader.Services
{
	public interface IArticleDependencyService
	{
		Dictionary<int, int[]> GetAffectedProducts(int articleId, Dictionary<int, ChangedValue> changedFields, bool isLive = false, int? definitionId = null);
	}

	public class ChangedValue
	{
		public string OldValue;

		public string NewValue;
	}
}
