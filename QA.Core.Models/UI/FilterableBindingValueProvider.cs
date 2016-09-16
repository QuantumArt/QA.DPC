using System;
using System.Linq;
using QA.Core.Models.Entities;
using QA.Core.Models.Processors;

namespace QA.Core.Models.UI
{
    public class FilterableBindingValueProvider : ReflectedBindingValueProvider
    {
        protected override object GetValueInternal(DependencyProperty prop, BindingExression be, object context)
        {
            if (!(context is Article))
            {
                throw new ArgumentException("context must be Article");
            }

            return DPathProcessor.Process(be.Expression, (Article)context).Select(mop => mop.ModelObject).ToArray();
        }
    }
}
