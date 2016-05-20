using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public interface ITransaction : IDisposable
	{
		void Commit();
	}
}