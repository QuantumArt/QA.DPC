using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public interface ITask
    {
        void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext);
    }
}
