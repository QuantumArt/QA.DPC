using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class DefinitionElement : DefinitionPathInfo
	{
        [Display(Name="InDefinition", ResourceType = typeof(ControlStrings))]
		public bool InDefinition { get; set; }
	}
}