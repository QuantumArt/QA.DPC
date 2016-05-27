using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace QA.Core.Models.UI
{
    public abstract class DependencyObject : IDependencyPropertyStorage, IDataContextProvider, IDataContextProviderSetter
    {
        private readonly Dictionary<DependencyProperty, ValueEntry> _valueTable;
        private readonly Dictionary<DependencyProperty, BindingExression> _bindings;
        private object _localDataContext;
        private IDataContextProvider _overridenProvider;

        public DependencyObject()
        {
            _valueTable = new Dictionary<DependencyProperty, ValueEntry>();
            _bindings = new Dictionary<DependencyProperty, BindingExression>();
        }

	    public DependencyObject Parent { get; internal set; }

        public object DataContext
        {
            get
            {
                return (_overridenProvider ?? (IDataContextProvider)this).GetDataContext(0);
            }
            set
            {
                _localDataContext = value;
            }
        }

        #region IDependencyPropertyStorage Members

        public void SetValue(DependencyProperty property, object value)
        {
            SetValueInternal(property, value);
        }

        public object GetValue(DependencyProperty property)
        {
            return GetValueInternal(property);
        }

        public T GetValue<T>(DependencyProperty property)
        {
            return (T)GetValueInternal(property);
        }

        #endregion

        internal void RegisterBinding(DependencyProperty dp, BindingExression bindingExpression)
        {
            // получим dependencyproperty
            if (dp == null)
                throw new ArgumentNullException("dp");

            if (dp.Log)
            {
                var logger = ObjectFactoryBase.Resolve<ILogger>();
                logger.Debug(string.Format("for {0} {1}.{2} {3} list: {4}", dp, dp.OwnerType, dp.PropertyType, bindingExpression.Expression, _bindings.GetHashCode()));
            }

            _bindings[dp] = bindingExpression;

            // удаляем текущее значение, если оно было
            _valueTable[dp] = new ValueEntry(DependencyProperty.UnsetValue);
        }

        internal DependencyProperty GetDependencyProperty(string name)
        {
            // получим dependencyproperty
            return DependencyProperty.FindByName(name, this.GetType());
        }



        internal virtual object GetValueInternal(DependencyProperty dp, bool returnUnsetValue = false)
        {
            var value = DependencyProperty.UnsetValue;
            ILogger logger = null;

            if (dp.Log) logger = ObjectFactoryBase.Resolve<ILogger>();

            if (dp.Log) logger.Debug(string.Format("{0} {1} {2}", dp.Name, dp.PropertyType, dp.GetHashCode()));

            if (value == DependencyProperty.UnsetValue)
            {
                // получаем из биндинга
                BindingExression binding;
                if (_bindings.TryGetValue(dp, out binding))
                {
                    // вот тут у нас есть выражение типа Product.Id
	                var bindingProvider = binding.BindingValueProvider ?? BindingValueProviderFactory.Current.GetBindingValueProvider(this);
                    
					value = bindingProvider.GetValue(dp, binding, this);
                }
                else
                {
                    if (dp.Log) logger.Debug(string.Format("binding not found for {0} {1} {2}", dp.Name, dp.PropertyType, dp.GetHashCode()));
                }
            }

            // получаем из локальных значений
            if (value == DependencyProperty.UnsetValue)
            {
                ValueEntry entry;
                if (_valueTable.TryGetValue(dp, out entry))
                {
                    value = entry.Value;
                }
                else
                {
                    if (dp.Log) logger.Debug(string.Format("local entry not found for {0} {1} {2}", dp.Name, dp.PropertyType, dp.GetHashCode()));
                }
            }

            // получаем по иерархии для объектов-наследников данного типа, находящихся выше по иерархии

            if (value == DependencyProperty.UnsetValue && dp.Inherit && Parent != null)
            {
                value = this.Parent.GetValueInternal(dp, returnUnsetValue: true);
            }

            if (!returnUnsetValue && value == DependencyProperty.UnsetValue)
            {
                // берем по умолчанию

                value = dp.GetDefaultValue();
            }

            return value;
        }

        internal virtual void SetValueInternal(DependencyProperty dp, object value)
        {
            if (value != null && value.GetType() != dp.PropertyType && !dp.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException("The value should be of type " + dp.PropertyType, "value");
            }

            //var uiElement = value as UIElement;

            //if (uiElement != null && uiElement.Parent == null)
            //{
            //    uiElement.Parent = this;
            //}

            //ValueEntry entry;
            //if (_valueTable.TryGetValue(dp, out entry))
            //{
            //    var ve = entry.Value;
            //    if (ve == DependencyProperty.UnsetValue && value == null)
            //    {
            //        // что делать c value types?
            //        return;
            //    }
            //}

            _valueTable[dp] = new ValueEntry(value);
        }

        #region IComponentConnector Members

        #endregion

        #region IDataContextProvider Members

        object IDataContextProvider.GetDataContext(int skip)
        {
            if (_overridenProvider != null)
                return _overridenProvider.GetDataContext(skip);
            return GetDataContextList().Skip(skip).FirstOrDefault();
        }

        object IDataContextProvider.GetRootContext(int skip)
        {

            if (_overridenProvider != null)
                return _overridenProvider.GetRootContext(skip);

            var list = GetDataContextList();

            return list.Skip(skip).LastOrDefault();
        }

        object IDataContextProvider.GetDataContext(BindingExression expr)
        {
            if (_overridenProvider != null)
                return _overridenProvider.GetDataContext(expr);

            var list = GetDataContextList().Skip(expr.Offset);

            if (expr.IsAbsolute)
                return list.LastOrDefault();
            else
            {
                return list.FirstOrDefault();
            }
        }

        private IEnumerable<object> GetDataContextList()
        {
            // ищем контекст в дереве.
            DependencyObject node = this;
            while (node != null)
            {
                if (node._localDataContext != null)
                {
                    yield return node._localDataContext;
                }

                node = node.Parent;
            }
        }

        #endregion

        #region IDataContextProviderSetter Members

        void IDataContextProviderSetter.ApplyProvider(IDataContextProvider provider)
        {
            _overridenProvider = provider;
        }

        #endregion
    }
}
