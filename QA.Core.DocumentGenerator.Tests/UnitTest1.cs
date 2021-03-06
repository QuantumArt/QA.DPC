﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using GemBox.Document;
using NUnit.Framework;
using Moq;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DocumentGenerator.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class UnitTest1
    {

        [OneTimeSetUp]
        public static void ClassInitialize()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            NotesServiceMoq.Setup(x => x.GetNoteText(It.IsAny<int>())).Returns<int>(x => $"Тестовая заметка с id={x}.");

            _xmlText = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.Product.xml")).ReadToEnd();
        }

        private static readonly Mock<INotesService> NotesServiceMoq = new Mock<INotesService>();
        private static string _xmlText;

        [Test]
        public void TestMailMerge()
        {
            var generator = new DocumentGenerator(NotesServiceMoq.Object);

            const string resFilePath = "c:\\temp\\TestMailMerge.pdf";

            generator.SaveAsPdf(_xmlText, Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestMailMerge.docx"), new FileStream(resFilePath, FileMode.Create));

        }

        [Test]
        public void TestNotes()
        {
            var generator = new DocumentGenerator(NotesServiceMoq.Object);

            const string resFilePath = "c:\\temp\\TestNotes.pdf";

            generator.SaveAsPdf(_xmlText, Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestNotes.docx"), new FileStream(resFilePath, FileMode.Create));

        }

        [Test]
        public void TestRazor()
        {
            var generator = new DocumentGenerator(NotesServiceMoq.Object);

            const string resFilePath = "c:\\temp\\TestRazor.pdf";

            generator.SaveAsPdf(_xmlText, Assembly.GetExecutingAssembly().GetManifestResourceStream("QA.Core.DocumentGenerator.Tests.TestDocs.TestRazor.docx"), new FileStream(resFilePath, FileMode.Create));

        }

    }
}
