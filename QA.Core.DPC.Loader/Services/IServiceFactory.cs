using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public interface IServiceFactory
	{
		FieldService GetFieldService();
		ArticleService GetArticleService();
		ContentService GetContentService();
	}
}