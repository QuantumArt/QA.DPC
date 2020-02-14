using System;
using NUnit.Framework;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestFixture]
    public class ServiceFactoryTests
    {
        #region Constants
        private const int UserId = 1;
        private const string ConnectionString = "Connection";
        #endregion

        #region Private properties
        private IServiceFactory Factory { get; set; }
        private IUserProvider UserProvider { get; set; }
        private IConnectionProvider ConnectionProvider { get; set; }
        #endregion

        #region Initialization

        [SetUp]
        public void Initialize()
        {
            ConnectionProvider = new ConnectionProviderFake(ConnectionString);
            UserProvider = new UserProviderFake {UserId = UserId};
            Factory = new ServiceFactory(ConnectionProvider, UserProvider);
        }

        #endregion

        #region Test methods

        [Test]
        public void Constructor_ConnectionIsNull_ThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var serviceFactory = new ServiceFactory(null, UserProvider);
            });
        }

        [Test]
        public void Constructor_UserProviderIsNull_ThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var service = new ServiceFactory(ConnectionProvider, null);
            });
        }

        [Test]
        public void GetArticleService_ValidConfiguration_ResolveService()
        {
            var service = Factory.GetArticleService();
            Assert.IsNotNull(service);
        }

        [Test]
        public void GetContentService_ValidConfiguration_ResolveService()
        {
            var service = Factory.GetContentService();
            Assert.IsNotNull(service);
        }

        [Test]
        public void GetFieldService_ValidConfiguration_ResolveService()
        {
            var service = Factory.GetFieldService();
            Assert.IsNotNull(service);
        }
        #endregion
    }
}