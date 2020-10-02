using System;
using System.IO;
using System.Threading.Tasks;
using QA.Core.DocumentGenerator;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
	public class PdfProductFormatter : IFormatter<Article>
	{
		private readonly IDocumentGenerator _documentGenerator;
		private readonly XmlProductFormatter _xmlFormatter;

		public PdfProductFormatter(IDocumentGenerator documentGenerator, XmlProductFormatter xmlFormatter)
		{
			_documentGenerator = documentGenerator;
			_xmlFormatter = xmlFormatter;
		}

		public Task<Article> Read(Stream stream)
		{
			throw new NotImplementedException();
		}

		public async Task Write(Stream stream, Article product)
		{
			using (var xmlStream = new MemoryStream())
			{
				await _xmlFormatter.Write(xmlStream, product);
				xmlStream.Position = 0;
				
				using (var xmlReader = new StreamReader(xmlStream))
				using (var pdfStream = new MemoryStream())
				{	
					string xml = xmlReader.ReadToEnd();
					_documentGenerator.SaveAsPdf(xml, null, pdfStream);
					pdfStream.Position = 0;
					await pdfStream.CopyToAsync(stream);
				}
			}
		}

		public string Serialize(Article product)
		{
			throw new NotImplementedException();
		}
	}
}
