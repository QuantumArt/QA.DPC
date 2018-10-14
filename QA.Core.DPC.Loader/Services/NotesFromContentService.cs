using System.Collections.Generic;
using System.Linq;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Services
{
    public class NotesFromContentService : INotesService
	{
		private readonly ISettingsService _settingsService;

		private int? _notesContentId;

		private string _notesTextFieldName;
		
		private readonly IArticleService _articleService;

		public NotesFromContentService(ISettingsService settingsService, IArticleService articleService)
		{
			_settingsService = settingsService;

			_articleService = articleService;
		}

		public string GetNoteText(int noteId)
		{
			InitTextFieldName();

			var textFieldVal = _articleService.Read(noteId).FieldValues.Single(x => x.Field.Name == _notesTextFieldName);

			return textFieldVal.Value;
		}

		private void InitTextFieldName()
		{
			if (_notesTextFieldName == null)
				_notesTextFieldName = _settingsService.GetSetting(SettingsTitles.NOTES_TEXT_FIELD_NAME);
		}

		public Dictionary<int,string> GetNoteTexts(int[] noteIds)
		{
			InitTextFieldName();

			return _articleService.List(0, noteIds)
				.Where(x => x.ContentId == NotesContentId && x.FieldValues.Any(y => y.Field.Name == _notesTextFieldName))
				.ToDictionary(x => x.Id, x => x.FieldValues.Single(y => y.Field.Name == _notesTextFieldName).Value);
		}

		public int NotesContentId
		{
			get
			{
				if (!_notesContentId.HasValue)
					_notesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.NOTES_CONTENT_ID));

				return _notesContentId.Value;
			}
		}
	}
}
