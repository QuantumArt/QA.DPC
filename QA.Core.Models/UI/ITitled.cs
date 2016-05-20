using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public interface ITitled<out T>
    {
        T Title { get; }
    }
}
