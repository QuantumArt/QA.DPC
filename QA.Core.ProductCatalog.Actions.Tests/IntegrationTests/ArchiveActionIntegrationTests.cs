﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using QA.Core.ProductCatalog.Actions.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
	[TestClass]
	public class ArchiveActionIntegrationTests : IntegrationTestsBase
	{	
		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_Archive_Product1()
		{
			var action = ActionFactory("ArchiveAction");
			var context = new ActionContext {ContentItemIds = new[] {2360}};
		    action.Process(context);
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_Archive_MarketingProduct1()
		{
			var action = ActionFactory("ArchiveAction");
			var context = new ActionContext {ContentItemIds = new[] {2423}};
		    action.Process(context);
		}
	}
}