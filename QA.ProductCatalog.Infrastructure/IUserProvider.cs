using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IUserProvider
    {
        int GetUserId();

        string GetUserName();
    }
}
