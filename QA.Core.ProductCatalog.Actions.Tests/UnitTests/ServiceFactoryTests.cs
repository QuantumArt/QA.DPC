using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestClass]
    public class ServiceFactoryTests
    {
        #region Constants
        private const int UserId = 1;
        private const string ConnectionString = "Connection";
        #endregion

        #region Private properties
        private IServiceFactory Factory { get; set; }
        private IUserProvider UserProvider { get; set; }
        #endregion

        #region Initialization
        [TestInitialize]
        public void Initialize()
        {
            UserProvider = new UserProviderFake { UserId = UserId };
            Factory = new ServiceFactory(ConnectionString, UserProvider);
        }
        #endregion

        #region Test methods
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ConnectionIsNull_ThrowException()
        {
            // ReSharper disable once UnusedVariable
            var serviceFactory = new ServiceFactory(null, UserProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_ConnectionIsEmpty_ThrowException()
        {
            // ReSharper disable once UnusedVariable
            var service = new ServiceFactory(string.Empty, UserProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_UserProviderIsNull_ThrowException()
        {
            // ReSharper disable once UnusedVariable
            var service = new ServiceFactory(ConnectionString, null);
        }

        [Ignore]
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetArticleService_InvalidConfiguration_ThrowException()
        {
            var service = Factory.GetArticleService();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void GetContentService_ValidConfiguration_ResolveService()
        {
            var service = Factory.GetContentService();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void GetFieldService_ValidConfiguration_ResolveService()
        {
            var service = Factory.GetFieldService();
            Assert.IsNotNull(service);
        }
        #endregion
    }
}