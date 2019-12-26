using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI.Controls
{
    public class NotesProcessor : NotesProcessorBase
	{
		private static readonly Regex NotesRegex = new Regex(@"&lt;note\s+id\=&quot;(\d+)&quot;\s*\/&gt;",
			RegexOptions.Compiled);

		private readonly Dictionary<int, byte> _localNoteIds = new Dictionary<int, byte>();
		private readonly INotesService _notesService;

		private byte _localNotesCounter = 1;

		private Note[] _notes;

		public NotesProcessor()
		{
			_notesService = ObjectFactoryBase.Resolve<INotesService>();
		}

		public int NotesContentId
		{
			get { return _notesService.NotesContentId; }
		}

		public Note[] Notes
		{
			get
			{
				if (_notes == null)
				{
					int[] noteIds = _localNoteIds.OrderBy(x => x.Value).Select(x => x.Key).ToArray();

					Dictionary<int, string> foundNotes = _notesService.GetNoteTexts(noteIds);

					_notes = new Note[noteIds.Length];

					for (int i = 0; i < noteIds.Length; i++)
					{
						int noteId = noteIds[i];

						_notes[i] = new Note
						{
							Id = noteId,
							Text = foundNotes.ContainsKey(noteId) ? foundNotes[noteId] : "Note with id=" + noteId + " not found"
						};
					}
				}

				return _notes;
			}
		}

		public override string ProcessTextWithNotes(string text)
		{
			if (text == null)
				return null;

			var notesInText = NotesRegex.Matches(text)
				.Cast<Match>()
				.Select(x => new {Text = x.Value, Id = int.Parse(x.Groups[1].Value)})
				.Distinct();

			foreach (var note in notesInText)
			{
				if (!_localNoteIds.ContainsKey(note.Id))
				{
					_localNoteIds.Add(note.Id, _localNotesCounter);

					_localNotesCounter++;
				}

				byte localNoteId = _localNoteIds[note.Id];

				const string noteFormat = "<span data-noteid=\"{1}\" class=\"Note\">{0}</span>";

				text = text.Replace(note.Text, string.Format(noteFormat, localNoteId, note.Id));
			}

			return text;
		}

		public class Note
		{
			public int Id { get; set; }

			public string Text { get; set; }
		}
	}
}