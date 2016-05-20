using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestClass]
	public class CloneActionTests : ActionTestsBase
	{
		#region Overrides
		protected override void InitializeAction()
		{
			Action = new CloneAction(ArticleService, FieldService, ProductService, Logger, CreateTransaction, null);
		}
		#endregion
	}
}
