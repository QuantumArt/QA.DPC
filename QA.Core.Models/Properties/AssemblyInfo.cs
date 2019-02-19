using System.Runtime.CompilerServices;
#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif

[assembly: XmlnsDefinition("http://artq.com/configuration", "QA.Core.Models")]
[assembly: XmlnsDefinition("http://artq.com/configuration", "QA.Core.Models.Configuration")]
[assembly: XmlnsDefinition("http://artq.com/configuration", "QA.Core.Models.ConceptModel")]
[assembly: XmlnsDefinition("http://artq.com/configuration", "QA.Core.Models.UI")]
[assembly: XmlnsDefinition("http://artq.com/configuration", "QA.Core.Models.UI.Converters")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "QA.Core.Models.UI")]

[assembly: InternalsVisibleTo("QA.Core.DPC.UI")]
[assembly: InternalsVisibleTo("QA.Core.DPC.Loader.Tests")]
