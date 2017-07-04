using System.Collections.Generic;

namespace QA.ProductCatalog.Infrastructure
{
	public interface INotesService
	{
		string GetNoteText(int noteId);

		Dictionary<int, string> GetNoteTexts(int[] noteIds);

		int NotesContentId { get; }
	}
}
