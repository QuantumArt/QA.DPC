using System.IO;

namespace QA.Core.DocumentGenerator
{
	public interface IDocumentGenerator
	{
		void SaveAsPdf(string xmlData, Stream docxTemplateStream, Stream pdfStream);
	}
}