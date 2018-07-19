using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Cache;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Tools;
using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Qp8Bll = Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Tests
{
    [Ignore]
    [TestClass]
    public class ProductLoaderTests
    {
        private const int ProductsContentId = 288;
        private const int ProductIdExisting = 699618;
        private const int ProductIdExistingService = 685329;
        private const int MproductIdExisting = 17432;
        private static IUnityContainer _container;

        [AssemblyInitialize]
        public static void StartUp(TestContext ctx)
        {
            // ReSharper disable once UnusedVariable
            var processRemoteValidationIf = new ProcessRemoteValidationIf { Condition = null };
            ctx.WriteLine("Started!");
            ctx.WriteLine("stub");
            _container = UnityConfig.Configure();
        }

        [TestInitialize]
        public void Init()
        {
            var connectinStringObject = ConfigurationManager.ConnectionStrings["qp_database"];
            var connectionString = connectinStringObject.ConnectionString;
            Do(connectionString);
        }

        [TestMethod]
        public void GetProductByIdTest()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var product = service.GetProductById(698473);
            Assert.IsNotNull(product);
            Assert.AreEqual(287, product.ContentId);
            Assert.AreEqual(MproductIdExisting, product.Id);
        }

        [TestMethod]
        public void Issue_failed_loading()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var faliedProduct = service.GetProductById(675494);
            Assert.IsNotNull(faliedProduct);
        }

        [TestMethod]
        public void GetProductByIdTest_2_times_with_timer()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var timer = new Stopwatch();
            timer.Start();
            service.GetProductById(ProductIdExisting);
            timer.Stop();
            Trace.WriteLine(timer.ElapsedMilliseconds);

            timer.Reset();
            timer.Start();
            service.GetProductById(ProductIdExisting);
            timer.Stop();

            Trace.WriteLine("Elapsed ms: " + timer.ElapsedMilliseconds);
            timer.Reset();

            _container.RegisterInstance<ICacheProvider>(new CacheProvider());
            _container.RegisterInstance<IVersionedCacheProvider>(new VersionedCacheProvider3());

            ObjectFactoryConfigurator.DefaultContainer = _container;
            service = ObjectFactoryBase.Resolve<IProductService>();

            timer.Start();
            service.GetProductById(ProductIdExisting);
            timer.Stop();
            Trace.WriteLine("Elapsed ms: " + timer.ElapsedMilliseconds);

        }

        [TestMethod]
        public void Test_Get_Product_By_Id_And_Serialize()
        {
            ProcessProduct(ProductIdExisting);
            ProcessProduct(ProductIdExistingService);
            ProcessProduct(MproductIdExisting);
        }

        [TestMethod]
        public void GetProductsByIdsTest()
        {
            var timer = new Stopwatch();
            var service = ObjectFactoryBase.Resolve<IProductService>();
            timer.Start();
            service.GetProductsByIds(ProductsContentId, new[] { ProductIdExisting, ProductIdExistingService, 3136 });
            timer.Stop();
            ObjectFactoryBase.Resolve<ILogger>().Info("Cold Elapsed ms: " + timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();
            service.GetProductsByIds(ProductsContentId, new[] { ProductIdExisting, ProductIdExistingService, 3136 });
            timer.Stop();
            ObjectFactoryBase.Resolve<ILogger>().Info("Hot Elapsed ms: " + timer.ElapsedMilliseconds);
        }

        [TestMethod]
        public void GetProductXmlByIdTest()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
            var content = service.GetProductById(ProductIdExisting);
            var xml = xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter);
            Assert.IsTrue(xml.Contains("<Type>"));
        }

        [TestMethod]
        public void GetSimpleProductXmlByIdTest()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
            var content = service.GetSimpleProductsByIds(new[] { ProductIdExisting })[0];
            var xml = xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter);
            Assert.IsTrue(xml.Contains("<Type>"));
        }

        [TestMethod]
        public void Profile_LoadStructureCache()
        {
            var connectinStringObject = ConfigurationManager.ConnectionStrings["qp_database"];
            var connectionString = connectinStringObject.ConnectionString;
            Do(connectionString);

            var timer = new Stopwatch();
            timer.Start();
            Do(connectionString);
            timer.Stop();
            ObjectFactoryBase.Resolve<ILogger>().Info("Cold Elapsed ms: " + timer.ElapsedMilliseconds);

        }

        [TestMethod]
        public void RegionTagTest()
        {
            ObjectFactoryBase.Resolve<IContentDefinitionService>();
            ObjectFactoryBase.Resolve<IRegionService>();
            ObjectFactoryBase.Resolve<IVersionedCacheProvider>();
            ObjectFactoryBase.Resolve<ICacheItemWatcher>();
            ObjectFactoryBase.Resolve<ISettingsService>();
            ObjectFactoryBase.Resolve<IUserProvider>();
            ObjectFactoryBase.Resolve<RegionTagService>();

            var serviceLoader = ObjectFactoryBase.Resolve<IProductService>();
            var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
            var product = serviceLoader.GetProductsByIds(288, new[] { /*PRODUCT_ID_CACHE_TEST,*/ 854754 });
            xmlservice.GetSingleXmlForProducts(product, ArticleFilter.DefaultFilter);
        }

        [TestMethod]
        public void RemoteMutualGroupValidatorTest()
        {
            var resourceDictrionary = Common.GetEmbeddedResourceText("QA.Core.DPC.Loader.RemoteValidators.Xaml.site_35.xaml");
            var validator = Common.GetEmbeddedResourceText("QA.Core.DPC.Loader.RemoteValidators.Xaml.content_309.xaml");
            var model = new Dictionary<string, string>
            {
                { "CONTENT_ITEM_ID", "2323" },
                { "STATUS_TYPE_ID", "32" },
                { "field_1279", "1234,5678" },
                { "field_1385", "44" }
            };
            var paramObject = new ValidationParamObject()
            {
                Model = model,
                Validator = validator,
                DynamicResource = resourceDictrionary
            };
            ValidationServices.ValidateModel(paramObject);
        }

        private static void ProcessProduct(int id)
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
            var content = service.GetProductById(id);
            var text = ConfigurationSerializer.GetXml(content);
            using (var file = File.CreateText(id + ".xaml"))
            {
                file.Write(text);
            }

            using (var file = File.CreateText(id + ".xml"))
            {
                file.Write(xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter));
            }
        }

        private static void Do(string connectionString)
        {
            using (new Qp8Bll.QPConnectionScope(connectionString))
            {
                var articleService = new ArticleService(connectionString, 1) { IsLive = false };
                articleService.LoadStructureCache();
            }
        }

        public void PublishAllProducts()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            ObjectFactoryBase.Resolve<IXmlProductService>();

            var nService = ObjectFactoryBase.Resolve<IQPNotificationService>();
            var ids = new List<int>();
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"select content_item_id from content_288 p inner join item_link L on L.item_id =p.content_item_id  and link_id=p.Regions where archive=0 and l.linked_item_id=1762", con))
                {
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            ids.Add((int)rd.GetDecimal(0));
                        }

                        rd.Close();
                    }
                }
            }

            var tasks = new List<Task>();
            foreach (var id in ids)
            {
                var content = service.GetProductById(id);
                ConfigurationSerializer.GetXml(content);
                var result = nService.SendProductsAsync(new[] { content }, true, "Admin", 1, false, false);
                tasks.Add(result);
            }

            var newT = Task.WhenAll(tasks);
            newT.Wait();
        }
    }
}
