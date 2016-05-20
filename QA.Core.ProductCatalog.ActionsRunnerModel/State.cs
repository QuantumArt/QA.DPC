using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
	public enum State : byte
	{
		New = 1,
		Running = 2,
		Done = 3,
		Failed = 4,
		Cancelled = 5
	}
}
