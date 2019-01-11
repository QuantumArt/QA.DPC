using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using GemBox.Document;
using GemBox.Document.MailMerging;
using QA.ProductCatalog.Infrastructure;
using RazorEngine;
using LoadOptions = GemBox.Document.LoadOptions;

namespace QA.Core.DocumentGenerator
{
    public class DocumentGenerator : IDocumentGenerator
	{
		private static readonly Regex NotesRegex = new Regex(@"<note\s+id\=[\'""”](\d+)[\'""”]\s*\/>", RegexOptions.Compiled);

		private DocumentModel _document;
		private readonly INotesService _notesService;


		public DocumentGenerator(INotesService notesService)
		{
			_notesService = notesService;
		}

		public void SaveAsPdf(string xmlData, Stream docxTemplateStream, Stream pdfStream)
		{
			ProcessTemplate(xmlData, docxTemplateStream);

			_document.Save(pdfStream, new PdfSaveOptions());
		}

		class LocalNote
		{
			public int Id;

			public string Text;
		}

		private void ProcessTemplate(string xmlData, Stream docxTemplateStream)
		{
			_document = DocumentModel.Load(docxTemplateStream, LoadOptions.DocxDefault);

			ProcessRazor(xmlData);

			ProcessMailMergeTemplate(xmlData);

			ProcessNotes();
		}

		private void ProcessRazor(string xmlData)
		{
			var textBoxElems = new List<TextBox>();

			TraverseElement(_document,
				ElementType.TextBox,
				x =>
				{
					textBoxElems.Add((TextBox) x);

					return true;
				},
				false);


			foreach (var textBoxElem in textBoxElems)
			{
				string elementText = textBoxElem.Content.ToString();

				var productXElement = XDocument.Parse(xmlData).Descendants("Product").First();

				string renderedText = Razor.Parse(elementText, productXElement, elementText.GetHashCode().ToString());

				var parentCollection = textBoxElem.ParentCollection;

				int textBoxIndexInCollection = parentCollection.IndexOf(textBoxElem);

				var runWithRenderedText = new Run(_document);

				parentCollection.Insert(textBoxIndexInCollection, runWithRenderedText);

				runWithRenderedText.Content.LoadText(renderedText, new HtmlLoadOptions());

				textBoxElem.Content.Delete();
			}
		}

		private void ProcessMailMergeTemplate(string xmlData)
		{
			//если нет полей то он из за RemoveEmptyRanges удалит весь контент
			//не надо вообще его вызывать тогда
			if (!CheckIfElementExists(ElementType.Field))
				return;

			var xpathDoc = new XPathDocument(new StringReader(xmlData));

			var navigator = xpathDoc.CreateNavigator();

			var rootContextInXml = (XPathNodeIterator)navigator.Evaluate("ProductInfo/Products/Product");

			if (rootContextInXml.Count == 0)
				throw new Exception("Не найден корневой продукт");

			_document.MailMerge.ClearOptions = MailMergeClearOptions.RemoveEmptyParagraphs |
			                                   MailMergeClearOptions.RemoveEmptyRanges |
			                                   MailMergeClearOptions.RemoveEmptyTableRows |
			                                   MailMergeClearOptions.RemoveUnusedFields;

			_document.MailMerge.Execute(new XPathMailMergeDataSource(rootContextInXml, navigator));
		}

		private bool CheckIfElementExists(ElementType elementType)
		{
			return !TraverseElement(_document, elementType, x => false, false);
		}

		/// <summary>
		/// обходит element ища все elementType и выполняя для каждого action
		/// </summary>
		/// <param name="element"></param>
		/// <param name="elementType"></param>
		/// <param name="action">если вернул false то обход прекращается</param>
		/// <param name="traverseInsideMatchedTypes">проходить ли внутрь элементов elementType после вызова action</param>
		/// <returns>был ли обход досрочно остановлен</returns>
		static bool TraverseElement(IContentElement element, ElementType elementType, Func<Element,bool> action, bool traverseInsideMatchedTypes)
		{
			foreach (var collection in element.Content)
			{
				foreach (var elementInCollection in collection)
				{
					bool traverseInsideCurrentElement = elementInCollection is IContentElement;
					
					if (elementInCollection.ElementType == elementType)
					{
						if (!action(elementInCollection))
							return false;

						if (!traverseInsideMatchedTypes)
							traverseInsideCurrentElement = false;
					}

					if (traverseInsideCurrentElement)
						if (!TraverseElement((IContentElement)elementInCollection, elementType, action, traverseInsideMatchedTypes))
							return false;
				}
			}

			return true;
		}

		private void ProcessNotes()
		{
			var notesGlobalToLocalDic = new Dictionary<int, LocalNote>();

			byte localNotesCounter = 1;

			//нельзя делать foreach так как при первом же LoadText итератор становится нерабочим и любые операции на возвращаемых им потом
			//объектах рождают эксепшены внутрях GemBox
			for (var contentRange = _document.Content.Find(NotesRegex).FirstOrDefault(); contentRange != null; contentRange = _document.Content.Find(NotesRegex).FirstOrDefault())
			{
				int noteId = int.Parse(NotesRegex.Match(contentRange.ToString()).Groups[1].Value);

				int localNoteId;

				if (notesGlobalToLocalDic.ContainsKey(noteId))
				{
					localNoteId = notesGlobalToLocalDic[noteId].Id;
				}
				else
				{
					string noteText = _notesService.GetNoteText(noteId);

					if (string.IsNullOrWhiteSpace(noteText))
						continue;

					localNoteId = localNotesCounter;

					notesGlobalToLocalDic[noteId] = new LocalNote { Id = localNoteId, Text = noteText };

					localNotesCounter++;
				}

				contentRange.LoadText(localNoteId.ToString(), new CharacterFormat { Superscript = true });
			}

			if (notesGlobalToLocalDic.Any())
			{
				var notesSection = _document.Sections.Last();

				var footer = new HeaderFooter(_document, HeaderFooterType.FooterFirst);

				foreach (var localNote in notesGlobalToLocalDic.Values)
				{
					footer.Content.End.InsertRange(
						new Run(_document, localNote.Id.ToString()) { CharacterFormat = new CharacterFormat { Superscript = true } }.Content);

					footer.Content.End.InsertRange(new Run(_document, " " + localNote.Text).Content);

					footer.Content.End.InsertRange(new SpecialCharacter(_document, SpecialCharacterType.LineBreak).Content);
				}

				notesSection.HeadersFooters.Add(footer);
			}
		}


		static DocumentGenerator()
		{
			ComponentInfo.SetLicense("D9RD-F99M-XT3X-2GGN");
		}
	}
}
