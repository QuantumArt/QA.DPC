using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class DefinitionInfoForFieldsSave<T> : DefinitionFieldInfo where T:Field
	{
		public new T Field { get; set; }
	}
}