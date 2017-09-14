using System;
using System.Collections.Generic;
using QA.Core.Logger;

namespace QA.Core.Models.UI
{
    public abstract class DependencyProperty
    {
        static readonly Dictionary<Type, Dictionary<string, DependencyProperty>> _registredProperties = new Dictionary<Type, Dictionary<string, DependencyProperty>>();
        static readonly List<DependencyProperty> _properties = new List<DependencyProperty>();

        static DependencyProperty() { }

        internal static readonly object UnsetValue = new object();

        #region Fields

        protected Type _ownerType;
        protected Type _propertyType;
        private string _name;
        internal uint uId;
        private bool _log;
        private bool _inherit;
        private bool _attached;

        #endregion

        #region Properties
        public string Name
        {
            get { return _name; }
        }
        public Type OwnerType
        {
            get { return _ownerType; }
        }

        public Type PropertyType
        {
            get { return _propertyType; }
        }

        public bool Log
        {
            get { return _log; }
        }

        public bool Inherit { get { return _inherit; } }

        #endregion

        internal static DependencyProperty FindByName(string name, Type instanceType)
        {
            Type owner = instanceType;
            while (owner.BaseType != null && owner != typeof(object))
            {
                Dictionary<string, DependencyProperty> dict = null;
                if (_registredProperties.TryGetValue(owner, out dict))
                {
                    DependencyProperty dp = null;
                    if (dict.TryGetValue(name, out dp))
                    {
                        return dp;
                    }
                }
                owner = owner.BaseType;
            }

            return null;
        }

        public static DependencyProperty Register<TProperty, TOwner>(string propertyName)
        {
            return Register(propertyName, typeof(TProperty), typeof(TOwner));
        }


        public static DependencyProperty RegisterAttach(string propertyName,
            Type propertyType,
            Type ownerType,
            bool inherit = false,
            bool log = false)
        {
            return RegisterCommon(propertyName, propertyType, ownerType, inherit, log, true);
        }

        public static DependencyProperty Register(string propertyName,
            Type propertyType,
            Type ownerType,
            bool inherit = false,
            bool log = false)
        {
            return RegisterCommon(propertyName, propertyType, ownerType, inherit, log, false);
        }

        private static DependencyProperty RegisterCommon(string propertyName, Type propertyType, Type ownerType, bool inherit, bool log, bool attached)
        {
            var property = new CustomDependencyProperty();
            ILogger logger = null;
            if (log)
            {
                logger = ObjectFactoryBase.Resolve<ILogger>();
                logger.Debug(string.Format("register dp {0} - {2}.{1}", propertyName, propertyType, ownerType));
            }

            property._name = propertyName;
            property._ownerType = ownerType;
            property._propertyType = propertyType;
            property._log = log;
            property._inherit = inherit;
            property._attached = attached;

            Dictionary<string, DependencyProperty> dict = null;

            if (!_registredProperties.TryGetValue(ownerType, out dict))
            {
                dict = new Dictionary<string, DependencyProperty>();
                _registredProperties.Add(ownerType, dict);
            }

            dict.Add(propertyName, property);

            property.uId = (uint)_properties.Count;
            _properties.Add(property);

            return property;
        }

        public object GetDefaultValue()
        {
            if (_propertyType.IsValueType)
                return Activator.CreateInstance(_propertyType);

            return null;
        }

    }

}
