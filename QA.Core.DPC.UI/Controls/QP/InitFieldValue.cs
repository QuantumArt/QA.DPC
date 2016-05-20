using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class InitFieldValue : UIElement
    {
        static InitFieldValue()
        {
            ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(InitFieldValue));

            FieldProperty = DependencyProperty.Register("Field", typeof(object), typeof(InitFieldValue));

            FieldIdProperty = DependencyProperty.Register("FieldId", typeof(int?), typeof(InitFieldValue));
        }

        public bool AsArray { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Field
        {
            get { return (object)GetValue(FieldProperty); }
            set { SetValue(FieldProperty, value); }
        }

        public int? FieldId
        {
            get { return (int?)GetValue(FieldIdProperty); }
            set { SetValue(FieldIdProperty, value); }
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty;

        public static readonly DependencyProperty FieldProperty;

        public static readonly DependencyProperty FieldIdProperty;
    }
}
