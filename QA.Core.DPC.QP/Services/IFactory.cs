using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.QP.Services
{
    public interface IFactory<T> where T : class
    {
        T Create();
    }
}
