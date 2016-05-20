using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.UI
{
    public interface IDependencyPropertyStorage
    {
        void SetValue(DependencyProperty property, object value);
        object GetValue(DependencyProperty property);
    }
}
