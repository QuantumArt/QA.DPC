using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DocumentGenerator.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext ctx)
		{
			NotesServiceMoq.Setup(x => x.GetNoteText(It.IsAny<int>())).Returns<int>(x => string.Format("Тестовая заметка с id={0}.", x));

			XmlText =
				new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.Product.xml"))
					.ReadToEnd();
		}

		private static readonly Mock<INotesService> NotesServiceMoq = new Mock<INotesService>();
		private static string XmlText;


		//[TestMethod]
		public void TestMailMerge()
		{
			var generator = new DocumentGenerator(NotesServiceMoq.Object);

			string resFilePath = "c:\\temp\\TestMailMerge.pdf";

			generator.SaveAsPdf(XmlText, Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestMailMerge.docx"), new FileStream(resFilePath, FileMode.Create));

			Process.Start(resFilePath);
		}

		//[TestMethod]
		public void TestNotes()
		{
			var generator = new DocumentGenerator(NotesServiceMoq.Object);

			string resFilePath = "c:\\temp\\TestNotes.pdf";

			generator.SaveAsPdf(XmlText,Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestNotes.docx"),new FileStream(resFilePath, FileMode.Create));

			Process.Start(resFilePath);
		}

		[TestMethod]
		public void TestRazor()
		{
			var generator = new DocumentGenerator(NotesServiceMoq.Object);

			string resFilePath = "c:\\temp\\TestRazor.pdf";

			generator.SaveAsPdf(XmlText, Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestRazor.docx"),new FileStream(resFilePath, FileMode.Create));

			Process.Start(resFilePath);
		}

	}
}
