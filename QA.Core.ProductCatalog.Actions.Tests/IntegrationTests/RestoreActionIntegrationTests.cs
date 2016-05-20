﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using QA.Core.ProductCatalog.Actions.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
	[TestClass]
	public class RestoreActionIntegrationTests : IntegrationTestsBase
	{	
		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_Restore_Product1()
		{
			var action = ActionFactory("RestoreAction");
			var context = new ActionContext {ContentItemIds = new[] {2360}};
		    action.Process(context);
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_Restore_MarketingProduct1()
		{
			var action = ActionFactory("RestoreAction");
			var context = new ActionContext {ContentItemIds = new[] {2423}};
		    action.Process(context);
		}
	}
}