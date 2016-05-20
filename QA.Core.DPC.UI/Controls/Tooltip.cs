using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class Tooltip : UIControl
    {

        private object _target;
        public object Target
        {
            get
            {
                return _target;
            }
            set
            {
                var element = value as UIElement;
                if (element != null)
                {
                    element.Parent = this;
                }

                _target = value;
            }
        }

        static Tooltip()
        {
            AutoHideProperty = DependencyProperty.Register("AutoHide", typeof(bool), typeof(Tooltip));
        }

        public bool AutoHide
        {
            get { return (bool)GetValue(AutoHideProperty); }
            set { SetValue(AutoHideProperty, value); }
        }

        public TooltipPosition Position { get; set; }
        public TooltipShowMode ShowOn { get; set; }
        public static readonly DependencyProperty AutoHideProperty;
    }

    public enum TooltipPosition
    {
        Top = 0,
        Right,
        Left,
        Bottom
    }

    public enum TooltipShowMode
    {
        MouseEnter = 0, Click
    }
}
