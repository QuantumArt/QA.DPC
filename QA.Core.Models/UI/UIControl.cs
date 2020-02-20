#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif

namespace QA.Core.Models.UI
{
	[ContentProperty("Content")]
	public abstract class UIControl : UIElement
	{
		private object _content;

		public object Content
		{
			get { return _content; }
			set
			{
				var element = value as UIElement;
				if (element != null)
				{
					element.Parent = this;
				}

				_content = value;
			}
		}
	}
}