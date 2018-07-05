using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class DefinitionContentInfo : DefinitionElement
	{
		public Content Content { get; set; }

		public bool IsFromDictionaries { get; set; }

		public bool AlreadyCachedAsDictionary { get; set; }
	}
}