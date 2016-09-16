using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using QA.Core.Extensions;

namespace QA.Core.Models.UI
{
    public class ReflectedBindingValueProvider : IBindingValueProvider
    {
        private static readonly ConcurrentDictionary<string, IPropertyAccessor> Accessors = new ConcurrentDictionary<string, IPropertyAccessor>();

        object IBindingValueProvider.GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {
            var value = GetValue(prop, be, source);
            value = ApplyConverter(prop, be, value, source);
            return value;
        }

        protected static object ApplyConverter(DependencyProperty dp, BindingExression be, object value, IDataContextProvider source)
        {
            if (be.Converter != null && value != DependencyProperty.UnsetValue)
            {
                var setter = be.Converter as IDataContextProviderSetter;
                setter?.ApplyProvider(source);

                value = be.Converter.Convert(value, dp.PropertyType, be.Parameter, CultureInfo.CurrentCulture);
                if (value == null)
                {
                    return dp.GetDefaultValue();
                }

            }
            return value;
        }

        protected virtual object GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {
            var dc = source?.GetDataContext(be);
            if (dc == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return be.Expression == "$DataContext" ? dc : GetValueInternal(prop, be, dc);
        }

        protected virtual object GetValueInternal(DependencyProperty prop, BindingExression be, object context)
        {
            return GetHierarchicalMemberInvocationValue(prop, be, context, be.Expression);
        }

        protected object GetHierarchicalMemberInvocationValue(DependencyProperty prop, BindingExression be, object context, string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                return context;
            }

            var array = targetName.SplitString('.').ToList();
            var value = context;
            foreach (var memberAccess in array)
            {
                if (value != null)
                {
                    value = GetMemberValue(prop, be, value, value.GetType(), memberAccess);
                }
            }

            return value;
        }

        private static object GetMemberValue(DependencyProperty prop, BindingExression be, object context, Type type, string targetName)
        {
            var hashcode = ComputeHash(prop, be, type, targetName);
            var fpa = Accessors.GetOrAdd(hashcode, p => new FastPropertyAccessor(type, targetName, be.BindingMode == BindingMode.Readonly));
            return fpa.GetValue(context);
        }

        private static string ComputeHash(DependencyProperty prop, BindingExression be, Type type, string path)
        {
            return "_hash" + prop.uId + "_" + be.GetHashCode() + "!" + type.GetHashCode() + prop.OwnerType + "-" + be.BindingMode + "_" + path;
        }
    }
}
