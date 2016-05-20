using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class ArticleChangedNotification
	{
		[AllowHtml]
		public string New_Xml { get; set; }

		[AllowHtml]
		public string Old_Xml { get; set; }

	}
}