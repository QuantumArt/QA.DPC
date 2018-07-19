using System.ComponentModel;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class DefinitionElement : DefinitionPathInfo
	{
		[DisplayName("Включить в описание")]
		public bool InDefinition { get; set; }
	}
}