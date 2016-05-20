using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class DefinitionElement : DefinitionPathInfo
	{
		[DisplayName("Включить в описание")]
		public bool InDefinition { get; set; }
	}
}