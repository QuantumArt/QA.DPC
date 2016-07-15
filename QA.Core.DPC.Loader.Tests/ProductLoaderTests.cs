using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.DPC.Loader;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Data;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Core.DPC.Loader.Tests
{
	using Qp8Bll = Quantumart.QP8.BLL;
	using QA.Core.Models.Configuration;
	using QA.Core.Models;
	using QA.Validation.Xaml;
	using QA.Validation.Xaml.Extensions.Rules;

	[TestClass]
	public class ProductLoaderTests
	{
		[AssemblyInitialize]
		public static void StartUp(TestContext ctx)
		{
			ProcessRemoteValidationIf stub = new ProcessRemoteValidationIf();
			stub.Condition = null;
			ctx.WriteLine("Started!");
			ctx.WriteLine("stub");
			_container = UnityConfig.Configure();
		}

		[TestInitialize]
		public void Init()
		{
			var connectinStringObject = ConfigurationManager.ConnectionStrings["qp_database"];

			var _connectionString = connectinStringObject.ConnectionString;

			Do(_connectionString);
		}

		#region Константы
		private const int PRODUCTS_CONTENT_ID = 288;
		private const int PRODUCT_ID_EXISTING = 699618;
        private const int PRODUCT_ID_EXISTING_Service = 685329;
		private const int MPRODUCT_ID_EXISTING = 17432;
		private const int PRODUCT_ID_CACHE_TEST = 2405;

		private static Microsoft.Practices.Unity.IUnityContainer _container;
		#endregion

		[TestMethod]
		public void GetProductByIdTest()
		{
			//_container.Resolve<>
			var service = ObjectFactoryBase.Resolve<IProductService>();

			var product = service.GetProductById(698473);
			Assert.IsNotNull(product);
			Assert.AreEqual(287, product.ContentId);
			Assert.AreEqual(MPRODUCT_ID_EXISTING, product.Id);
		}


		[TestMethod]
		public void Issue_failed_loading()
		{
			//_container.Resolve<>
			var service = ObjectFactoryBase.Resolve<IProductService>();

			var faliedProduct = service.GetProductById(675494);
			Assert.IsNotNull(faliedProduct);
		}
		[TestMethod]
		public void GetProductByIdTest_2_times_with_timer()
		{
			//_container.Resolve<>
			var service = ObjectFactoryBase.Resolve<IProductService>();

			var timer = new Stopwatch();

			timer.Start();
			var content = service.GetProductById(PRODUCT_ID_EXISTING);

			timer.Stop();

			Trace.WriteLine(timer.ElapsedMilliseconds);

			timer.Reset();
			timer.Start();
			content = service.GetProductById(PRODUCT_ID_EXISTING);

			timer.Stop();


			//service.GetProductById(3136);
			//service.GetProductById(2405);

			Trace.WriteLine("Elapsed ms: " + timer.ElapsedMilliseconds);
			timer.Reset();

			_container.RegisterInstance<ICacheProvider>(new CacheProvider());
			_container.RegisterInstance<IVersionedCacheProvider>(new VersionedCacheProvider3());

			ObjectFactoryConfigurator.InitializeWith(_container);

			service = ObjectFactoryBase.Resolve<IProductService>();

			timer.Start();
			content = service.GetProductById(PRODUCT_ID_EXISTING);
			timer.Stop();
			Trace.WriteLine("Elapsed ms: " + timer.ElapsedMilliseconds);

		}

		[TestMethod]
		public void Test_Get_Product_By_Id_And_Serialize()
		{
			ProcessProduct(PRODUCT_ID_EXISTING);
			ProcessProduct(PRODUCT_ID_EXISTING_Service);
			ProcessProduct(MPRODUCT_ID_EXISTING);
		}

		private static void ProcessProduct(int id)
		{
			var service = ObjectFactoryBase.Resolve<IProductService>();
			var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();

			var content = service.GetProductById(id);

			var text = QA.Core.Models.Tools.ConfigurationSerializer.GetXml(content);

			// generate xml of full model
			using (var file = File.CreateText(id + ".xaml"))
			{
				file.Write(text);
			}

			// generate compact xml without schema information

			using (var file = File.CreateText(id + ".xml"))
			{
				file.Write(xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter));
			}
		}

		[TestMethod]
		public void GetProductsByIdsTest()
		{
			var timer = new Stopwatch();
			var service = ObjectFactoryBase.Resolve<IProductService>();
			timer.Start();
			var content = service.GetProductsByIds(PRODUCTS_CONTENT_ID, new int[] { PRODUCT_ID_EXISTING, PRODUCT_ID_EXISTING_Service, 3136 });
			timer.Stop();
			ObjectFactoryBase.Resolve<ILogger>().Info("Cold Elapsed ms: " + timer.ElapsedMilliseconds);
			timer.Reset();
			timer.Start();
			content = service.GetProductsByIds(PRODUCTS_CONTENT_ID, new int[] { PRODUCT_ID_EXISTING, PRODUCT_ID_EXISTING_Service, 3136 });
			timer.Stop();
			ObjectFactoryBase.Resolve<ILogger>().Info("Hot Elapsed ms: " + timer.ElapsedMilliseconds);
		}

		public void PublishAllProducts()
		{

			var service = ObjectFactoryBase.Resolve<IProductService>();
			var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
			var nService = ObjectFactoryBase.Resolve<IQPNotificationService>();

			//получить и опубликовать все продукты

			List<int> ids = new List<int>();
			using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString))
			{
				con.Open();

				using (SqlCommand cmd = new SqlCommand(
					@"select content_item_id from content_288 p 
                        inner join item_link L on L.item_id =p.content_item_id  and link_id=p.Regions
                        where archive=0 and l.linked_item_id=1762", con))
				{

					using (SqlDataReader rd = cmd.ExecuteReader())
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
			foreach (int id in ids)
			{
				var content = service.GetProductById(id);

				var text = QA.Core.Models.Tools.ConfigurationSerializer.GetXml(content);
				var result = nService.SendProductsAsync(new[] { content }, true, "Admin", 1, false);
				tasks.Add(result);
			}
			var newT = Task.WhenAll(tasks);
			newT.Wait();

		}

		[TestMethod]
		public void GetProductXmlByIdTest()
		{
			//_container.Resolve<>
			var service = ObjectFactoryBase.Resolve<IProductService>();
			var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
			var content = service.GetProductById(PRODUCT_ID_EXISTING);

			string xml = xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter);
            Assert.IsTrue(xml.Contains("<Type>"));
        }

        [TestMethod]
        public void GetSimpleProductXmlByIdTest()
        {
            //_container.Resolve<>
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
            var content = service.GetSimpleProductsByIds(new[] { PRODUCT_ID_EXISTING })[0];

            string xml = xmlservice.GetProductXml(content, ArticleFilter.DefaultFilter);
            Assert.IsTrue(xml.Contains("<Type>"));
        }

        [TestMethod]
		public void Profile_LoadStructureCache()
		{
			var connectinStringObject = ConfigurationManager.ConnectionStrings["qp_database"];

			var _connectionString = connectinStringObject.ConnectionString;

			Do(_connectionString);

			var timer = new Stopwatch();
			timer.Start();
			Do(_connectionString);
			timer.Stop();
			ObjectFactoryBase.Resolve<ILogger>().Info("Cold Elapsed ms: " + timer.ElapsedMilliseconds);

		}

		private static void Do(string _connectionString)
		{
			using (new Qp8Bll.QPConnectionScope(_connectionString))
			{

				var _articleService = new Quantumart.QP8.BLL.Services.API.ArticleService(_connectionString, 1);

				_articleService.IsLive = false;

				_articleService.LoadStructureCache();

			}
		}

		

		/// <summary>
		/// Получение структуры контента-словарая на основе структур XML с маппингом данных
		/// </summary>
		[TestMethod]
		public void RegionTagTest()
		{
			var serviceContnDef = ObjectFactoryBase.Resolve<IContentDefinitionService>();
			var serviceRegion = ObjectFactoryBase.Resolve<IRegionService>();
			var cacheProvider = ObjectFactoryBase.Resolve<IVersionedCacheProvider>();
			var cacheItemWatcher = ObjectFactoryBase.Resolve<Cache.ICacheItemWatcher>();
			var serviceSettings = ObjectFactoryBase.Resolve<ISettingsService>();
			var userProvider = ObjectFactoryBase.Resolve<IUserProvider>();
			var serviceRegionTags = ObjectFactoryBase.Resolve<RegionTagService>();

			var serviceLoader = ObjectFactoryBase.Resolve<IProductService>();
			var xmlservice = ObjectFactoryBase.Resolve<IXmlProductService>();
			//var content = serviceLoader.GetProductById(PRODUCT_ID_CACHE_TEST);

			//string xml = xmlservice.GetProductXml(content);
			//string xml = "";
			//var res = serviceRegionTags.GetTags(xml);

			var product = serviceLoader.GetProductsByIds(288, new int[] { /*PRODUCT_ID_CACHE_TEST,*/ 854754 }, false);
			var xml2 = xmlservice.GetSingleXmlForProducts(product, ArticleFilter.DefaultFilter);
			//695934
		}


		[TestMethod]
		//[TestCategory("xaml")]
		public void RemoteMutualGroupValidatorTest()
		{
			var resourceDictrionary = Common.GetEmbeddedResourceText("QA.Core.DPC.Loader.RemoteValidators.Xaml.site_35.xaml");
			var validator = Common.GetEmbeddedResourceText("QA.Core.DPC.Loader.RemoteValidators.Xaml.content_309.xaml");

			var model = new Dictionary<string, string>() 
                { 
                    { "CONTENT_ITEM_ID", "2323" },
                    { "STATUS_TYPE_ID", "32" },
                    { "field_1279", "1234,5678" },
                    { "field_1385", "44" },
                };

			var result = ValidationServices.ValidateModel(model, validator, resourceDictrionary);

			//Assert.IsFalse(result.IsValid);
			//Assert.IsNotNull(result.Messages);
			//Assert.IsNotNull(result.Result);

			//Assert.AreEqual(result.Result.Errors[0].Definition.PropertyName, "field_1237");
			//Assert.AreEqual(result.Result.Errors[0].Message, "Если дата указана, то она должна быть не ранее 2012.03.03");

			//Assert.AreEqual(1, result.Result.Errors.Count);
		}
	}
}
