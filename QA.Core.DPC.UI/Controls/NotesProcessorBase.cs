using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
	public abstract class NotesProcessorBase : UIElement
	{
		public abstract string ProcessTextWithNotes(string text);

		static NotesProcessorBase()
		{
			NotesProcessorProperty = DependencyProperty.RegisterAttach("NotesProcessor", typeof (NotesProcessorBase), typeof (NotesProcessorBase), true);
		}

		private static readonly DependencyProperty NotesProcessorProperty;

		public static NotesProcessorBase GetNotesProcessor(DependencyObject obj)
		{
			return (NotesProcessorBase)obj.GetValue(NotesProcessorProperty);
		}

		public static void SetNotesProcessor(DependencyObject obj, NotesProcessorBase value)
		{
			obj.SetValue(NotesProcessorProperty, value);
		}
	}
}
