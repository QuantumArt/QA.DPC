using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.ProductCatalog.Infrastructure
{
	public interface INotesService
	{
		string GetNoteText(int noteId);

		Dictionary<int, string> GetNoteTexts(int[] noteIds);

		int NotesContentId { get; }
	}
}
