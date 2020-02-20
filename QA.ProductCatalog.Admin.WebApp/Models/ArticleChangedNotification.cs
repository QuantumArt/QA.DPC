using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class ArticleChangedNotification
	{
		public string New_Xml { get; set; }

		public string Old_Xml { get; set; }

	}
}