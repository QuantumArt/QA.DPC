using System;
using Portable.Xaml.Markup;
using QA.Configuration;

namespace QA.Core.Models.UI
{
    public abstract class UIElement : DependencyObject, IResourceContainer
    {
        private static readonly DependencyProperty NameProperty = DependencyProperty.Register<string, UIElement>("Name");
        private static readonly DependencyProperty HiddenProperty = DependencyProperty.Register<bool, UIElement>("Hidden");
        private static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register<string, UIElement>("ClassName");

        static UIElement() { }

        private ResourceDictionary _resources;
        private Rectangle _padding;
        private readonly string _uId = "uid_" + Guid.NewGuid();

        public string UId
        {
            get { return _uId; }
        }


        public string Name
        {
            get
            {
                return GetValue<string>(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }

        public string ClassName
        {
            get { return (string)GetValue(ClassNameProperty); }
            set { SetValue(ClassNameProperty, value); }
        }

        public bool? Hidden
        {
            get { return (bool)GetValue(HiddenProperty); }
            set { SetValue(HiddenProperty, value); }
        }

        public Rectangle Padding
        {
            get
            {
                return _padding;
            }

            set
            {
                _padding = CorrectValue(value);
            }
        }

        #region IResourceContainer Members

        /// <summary>
        /// Ресурсный словарь.
        /// </summary>
        [Ambient]
        public ResourceDictionary Resources
        {
            get
            {
                if (_resources == null)
                {
                    _resources = new ResourceDictionary(this);
                }

                return _resources;
            }
        }

        public bool HasResources
        {
            get
            {
                return _resources != null && _resources.Count > 0;
            }
        }



        public bool TryGetResource(object key, out object value)
        {
            if (!HasResources)
            {
                value = null;
                return false;
            }

            return _resources.TryGetValue(key, out value);
        }

        #endregion

        public virtual T OnGetItem<T>(T element)
        {
            return element;
        }

        protected object CorrectValue(object value)
        {
            return CorrectValue<object>(value);
        }

        protected T CorrectValue<T>(T value)
        {
            object val;
            val = value;
            if (val is DependencyObject)
            {
                ((DependencyObject)val).Parent = this;
            }
            return (T)val;
        }
    }


}
