﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public abstract class BindingValueProvider:IBindingValueProvider
    {
        object IBindingValueProvider.GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {
            return OnGetValue(prop, be, source);
        }

        protected abstract object OnGetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source);

    }
}
