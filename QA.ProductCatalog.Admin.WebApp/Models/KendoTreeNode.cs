using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class KendoTreeNode
	{
		public string text { get; set; }

		// if specified, renders the item as a link. (<a href=""></a>)
		public string url { get; set; }

		// renders a <img class="k-image" src="/images/icon.png" />
		public string imageUrl { get; set; }

		// renders a <span class="k-sprite icon save" />
		public string spriteCssClass { get; set; }

		// specifies whether the node text should be encoded or not
		// useful when rendering node-specific HTML
		public bool encoded = true;

		// specifies whether the item is initially expanded
		// (applicable when the item has child nodes)
		public bool expanded = true;

		// specifies whether the item checkbox is initially checked
		// (applicable for items with checkboxes using the default checkbox template)
		public bool @checked = false;

		// specifies whether the item is initially selected
		public bool selected = false;

		public bool hasChildren { get; set; }
	}
}