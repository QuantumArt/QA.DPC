using System;
using QA.Core.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace QA.Core.Models.UI
{
    public class ReflectedBindingValueProvider : IBindingValueProvider
    {
        static readonly ConcurrentDictionary<string, IPropertyAccessor> Accessors = new ConcurrentDictionary<string, IPropertyAccessor>();

        #region IBindingValueProviderFactory Members

        object IBindingValueProvider.GetValue(DependencyProperty prop, BindingExression be, IDataContextProvider source)
        {
            var value = GetValue(prop, be, source);

            value = ApplyConverter(prop, be, value, source);

            return value;
        }

        protected static object ApplyConverter(DependencyProperty dp, 
            BindingExression be, 
            object value, 
            IDataContextProvider source)
        {
            if (be.Converter != null && value != DependencyProperty.UnsetValue)
            {
                var setter = be.Converter as IDataContextProviderSetter;
                if (setter != null) 
                {
                    setter.ApplyProvider(source);
                }

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
			if(source == null)
				return DependencyProperty.UnsetValue;

            var dc = source.GetDataContext(be);
            
			if (dc == null)
                return DependencyProperty.UnsetValue;

            if (be.Expression == "$DataContext")
                return dc;

            return GetValueInternal(prop, be, dc);
        }

        protected virtual object GetValueInternal(DependencyProperty prop, BindingExression be, object context)
        {
            var targetName = be.Expression;

            return GetHierarchicalMemberInvocationValue(prop, be, context, targetName);
        }

        protected object GetHierarchicalMemberInvocationValue(DependencyProperty prop, BindingExression be, object context, string targetName)
        {
            if (string.IsNullOrEmpty(targetName))
            {
                return context;
            }

            var array = targetName.SplitString('.').ToList();

            object value = context;

            foreach (var memberAccess in array)
            {
                if (value != null)
                {
                    value = GetMemberValue(prop, be, value, value.GetType(), memberAccess);
                }
            }

            return value;
        }

        private object GetMemberValue(DependencyProperty prop, BindingExression be, object context, Type type, string targetName)
        {
            var hashcode = ComputeHash(prop, be, type, targetName);

            var fpa = Accessors
                .GetOrAdd(hashcode, p => new FastPropertyAccessor(type, targetName, be.BindingMode == BindingMode.Readonly));

            return fpa.GetValue(context);
        }

        private static string ComputeHash(DependencyProperty prop, BindingExression be, Type type, string path)
        {
            // хэш учитывает тип источника, выражение биндинга, поле, режим биндинга
            var hashcode = "_hash" + prop.uId + "_" + be.GetHashCode() + "!" + type.GetHashCode() + prop.OwnerType + "-" + be.BindingMode + "_" + path;
            return hashcode;
        }

        #endregion
    }
}
