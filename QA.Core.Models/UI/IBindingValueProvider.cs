using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public interface IBindingValueProvider
    {
        object GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source);
    }
}
