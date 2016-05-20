using Quantumart.QP8.BLL;
using System;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class ArticleEventArgs : EventArgs
	{
		public Article Article { get; private set; }

		public ArticleEventArgs(Article article)
		{
			Article = article;
		}
	}
}