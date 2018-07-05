using System;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public interface ITransaction : IDisposable
	{
		void Commit();
	}
}