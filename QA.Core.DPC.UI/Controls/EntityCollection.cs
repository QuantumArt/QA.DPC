using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    public class EntityCollection : QPListControlBase
    {
        public QPBehavior Behavior { get; set; }

	    static EntityCollection()
	    {
			SeparatorTemplateProperty = DependencyProperty.Register("SeparatorTemplate", typeof(object), typeof(EntityCollection));
	    }

		public object SeparatorTemplate
		{
			get { return GetValue(SeparatorTemplateProperty); }
			set { SetValue(SeparatorTemplateProperty, CorrectValue(value)); }
		}

		public static readonly DependencyProperty SeparatorTemplateProperty;
    }
}
