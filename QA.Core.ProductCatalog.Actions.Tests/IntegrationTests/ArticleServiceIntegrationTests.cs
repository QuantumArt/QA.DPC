﻿using System;
using NUnit.Framework;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;
using Unity;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore("Manual")]
    [TestFixture]
    public class ArticleServiceIntegrationTests : IntegrationTestsBase
    {
        [Test]
        [Category("Integration")]
        public void CastomAction_ArticleService_Read()
        {
            var service = Container.Resolve<IArticleService>();
            var createTransaction = Container.Resolve<Func<ITransaction>>();

            using (var transaction = createTransaction())
            {
                var a1 = service.Read(0);
                var a2 = service.Read(2360);

                Assert.IsNull(a1);
                Assert.IsNotNull(a2);
                Assert.IsNotNull(a2.FieldValues);

                transaction.Commit();
            }
        }

        [Test]
        [Category("Integration")]
        public void CastomAction_ArticleService_Save()
        {
            var service = Container.Resolve<IArticleService>();
            var createTransaction = Container.Resolve<Func<ITransaction>>();

            using (var transaction = createTransaction())
            {
                var a = service.Read(2360);
                a = service.Save(a);
                transaction.Commit();

                Assert.IsNotNull(a);
            }
        }
    }
}