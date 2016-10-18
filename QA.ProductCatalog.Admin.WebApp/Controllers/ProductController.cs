using System;
using System.Diagnostics;
using System.Web.Routing;
using QA.Core;
using QA.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using QA.Core.DPC.Loader;
using QA.Core.DPC.UI;
using QA.Core.Models;
using QA.Core.Models.Extensions;
using QA.Core.Models.Entities;
using QA.Core.Models.Filters.Base;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.Web;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Processors;
using System.Globalization;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{

    public class ProductController : Controller
    {
        private readonly Func<string, string, IAction> _getAction;
        private IVersionedCacheProvider _versionedCacheProvider;
        private readonly Func<string, IArticleFormatter> _getFormatter;
        private readonly IProductLocalizationService _localizationService;

        public ProductController(Func<string, string, IAction> getAction, IVersionedCacheProvider versionedCacheProvider, Func<string, IArticleFormatter> getFormatter, IProductLocalizationService localizationService)
        {
            _getAction = getAction;
            _versionedCacheProvider = versionedCacheProvider;
            _getFormatter = getFormatter;
            _localizationService = localizationService;
        }

        [RequireCustomAction]
        public ActionResult Index(int content_item_id, bool live = false, string[] filters = null, bool includeRelevanceInfo = true)
        {
            if (content_item_id <= 0)
            {
                ViewBag.Message = "Параметры действия некорректны: content_item_id должен быть больше 0.";
                return View();
            }

            Stopwatch sw = Stopwatch.StartNew();

            var product = ObjectFactoryBase.Resolve<IProductService>().GetProductById(content_item_id, live);
            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View();
            }

            var productLoadedIn = sw.ElapsedMilliseconds;

            var control = ObjectFactoryBase.Resolve<IProductControlProvider>().GetControlForProduct(product);

            if (control == null)
            {
                ViewBag.Message = "Для указанного продукта не задано визуальное представление.";
                return View();
            }

            var controlLoadedIn = sw.ElapsedMilliseconds;


            var allFilter = new AllFilter(filters?.Select(ObjectFactoryBase.Resolve<IArticleFilter>).ToArray());

            product = ArticleFilteredCopier.Copy(product, allFilter);

            

            if (includeRelevanceInfo)
            {
                var relevanceService = ObjectFactoryBase.Resolve<IProductRelevanceService>();
                var relevanceItems = relevanceService.GetProductRelevance(product, live);
                var relevanceField = new MultiArticleField() { FieldName = "Relevance" };                          
                int id = 0;

                product.AddField(relevanceField);

                foreach (var relevanceItem in relevanceItems)
                {
                    var localizedRelevanceArticle = new Article();                    
                    
                    if (relevanceItem.Culture == CultureInfo.InvariantCulture)
                    {
                        AddRelevanceData(product, relevanceItem);
                    }
                    else
                    {
                        relevanceField.Items.Add(++id, localizedRelevanceArticle);
                        AddRelevanceData(localizedRelevanceArticle, relevanceItem);
                    }

                }

                bool isRelevant = relevanceItems.All(r => r.Culture == CultureInfo.InvariantCulture || r.Relevance == ProductRelevance.Relevant);
                if (!isRelevant)
                {
                    product.AddPlainField("IsRelevant", isRelevant, "Актуальность продукта");
                }
            }

            IModelPostProcessor processor = new HierarchySorter(new HierarchySorterParameter
            {
                Domain = 100,
                ConstructHierarchy = true,
                ParentRelativePath = "Parent",
                PathToCollection = "Parameters",
                PathToSortOrder = "SortOrder",
                PropertyToSet = "NewSortOrder"
            });

            product = processor.ProcessModel(product);

            product.AddArticle("Diagnostics",
                new Article()
                    .AddPlainField("ProductLoaded", productLoadedIn, "Продукт загружен")
                    .AddPlainField("ControlLoaded", controlLoadedIn, "Карточка получена")
                    .AddPlainField("RelevanceLoaded", sw.ElapsedMilliseconds, "Актуальность получена")
                    .AddPlainField("Stopwatch", sw));



            control.DataContext = product;

            return View("Index", control);
        }

        [RequireCustomAction]
        public ActionResult GetXml(int content_item_id, bool live = false)
        {
            var product = ObjectFactoryBase.Resolve<IProductService>().GetProductById(content_item_id, live);
            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View();
            }

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            var xml = ObjectFactoryBase.Resolve<IXmlProductService>().GetProductXml(product, filter);

            return View("GetXml", (object)xml);
        }

        public ActionResult GetProductData(int content_item_id, string formatter, bool live = false, string lang = null)
        {
            var product = ObjectFactoryBase.Resolve<IProductService>().GetProductById(content_item_id, live);
            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View();
            }

            if (lang != null)
            {
                var culture = CultureInfo.GetCultureInfo(lang);
                product = _localizationService.Localize(product, culture);
            }

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            var data = _getFormatter(formatter).Serialize(product, filter, true);

            return View(formatter, (object)data);
        }

        [RequireCustomAction]
        public ActionResult GetXmlFromConsumer(int content_item_id, bool live = false)
        {
            var consumerMonitoringServiceFunc = ObjectFactoryBase.Resolve<Func<bool, IConsumerMonitoringService>>();

            string xml = consumerMonitoringServiceFunc(live).GetProduct(content_item_id);

            return View("GetXml", (object)xml);
        }

        [RequireCustomAction]
        public ActionResult GetXmlDownloadJson(int content_item_id, bool live = false)
        {
            string oneTimeKey = Guid.NewGuid().ToString();

            _versionedCacheProvider.Add(true, oneTimeKey, new string[] { }, TimeSpan.FromMinutes(10));

            return
                Json(
                    new
                    {
                        Type = "Download",
                        Url = Url.Action("DownloadXml", "Product", new { content_item_id, live, oneTimeKey }, Request.Url.Scheme)
                    },
                    JsonRequestBehavior.AllowGet);
        }


        public ActionResult DownloadXml(int content_item_id, string oneTimeKey, bool live = false)
        {
            if (_versionedCacheProvider.Get(oneTimeKey) == null)
                return new HttpUnauthorizedResult("Access to the file is restricted because of invalid or missing key.");

            _versionedCacheProvider.Invalidate(oneTimeKey);

            var product = ObjectFactoryBase.Resolve<IProductService>().GetProductById(content_item_id, live);

            if (product == null)
                return HttpNotFound("Продукт не найден.");

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            var xml = ObjectFactoryBase.Resolve<IXmlProductService>().GetProductXml(product, filter);

            return File(Encoding.UTF8.GetBytes(xml),
                "text/xml",
                string.Format("{0}-{1}.xml", product.Id, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
        }

        [RequireCustomAction]
        public async Task<ActionResult> Action(string command, [ModelBinder(typeof(ActionContextModelBinder))] ActionContext context, string adapter)
        {
            if (!ModelState.IsValid)
            {
                return Error(ModelState);
            }

            try
            {
                var action = _getAction(command, adapter);
                string message;

                if (action is IAsyncAction)
                {
                    message = await ((IAsyncAction)action).Process(context);
                }
                else
                {
                    message = action.Process(context);
                }

                return Info(message);
            }
            catch (ActionException ex)
            {
                return Error(ex);
            }
            catch (ResolutionFailedException)
            {
                return Error("Не удалось найти обработчик для команды " + command);
            }
        }

        [RequireCustomAction]
        [HttpGet]
        public ActionResult Send()
        {
            SendProductModel model = new SendProductModel();
            return View(model);
        }

        [RequireCustomAction]
        [HttpPost]
        public ActionResult Publish(SendProductModel model)
        {
            PublishAction action = ObjectFactoryBase.Resolve<PublishAction>();
            int[] ids = null;
            if (model != null && ModelState.IsValid)
            {
                var idsList = model.ArticleIds.SplitString(' ', ',', ';', '\n', '\r').Distinct().ToArray();
                if (idsList.Length > 200)
                {
                    ModelState.AddModelError("", "Слишком много продуктов. Укажите не более 200");
                    return View(model);
                }
                try
                {
                    ids = idsList.Select(x => int.Parse(x)).ToArray();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Указаны нечисловые значения. " + ex.Message);
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var context = new ActionContext { ContentId = 288, ContentItemIds = ids };
                        action.Process(context);
                        model.Message = "Продукты успешно опубликованы и отправлены";
                    }
                    catch (ActionException ex)
                    {
                        model.Message = ex.Message;
                    }
                }
            }

            return View("Send", model);
        }

        #region Private methods
        private void AddRelevanceData(Article article, RelevanceInfo relevanceInfo)
        {

            string statusText = relevanceInfo.Relevance == ProductRelevance.Missing
                ? "Отсутствует на витрине"
                : relevanceInfo.Relevance == ProductRelevance.Relevant ? "Актуален" : "Содержит неотправленные изменения";

            article
                .AddPlainField("ConsumerCulture", relevanceInfo.Culture.NativeName, "Язык витрины")
                .AddPlainField("ConsumerStatusText", statusText, "Статус на витрине")
                .AddPlainField("ConsumerStatusCode", relevanceInfo.Relevance.ToString(), "Код статуса на витрине")
                .AddPlainField("ConsumerLastPublished", relevanceInfo.LastPublished.ToString(), "Дата последней публикации")
                .AddPlainField("ConsumerLastPublishedUserName", relevanceInfo.LastPublishedUserName, "Опубликовал");
        }

        private JsonResult Error(ModelStateDictionary modelstate)
        {
            var errors = from fv in modelstate
                         from e in fv.Value.Errors
                         select new { Field = fv.Key, Message = e.ErrorMessage };

            return this.Json(new { @Type = "Error", Text = "Validation", ValidationErrors = errors }, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Error(ActionException exception)
        {
            if (exception.InnerExceptions.Any())
            {
                StringBuilder sb = new StringBuilder("Не удалось обработать продукты:");

                foreach (ProductException ex in exception.InnerExceptions)
                {
                    sb.AppendLine();

                    string exText = ex.Message;

                    if (ex.InnerException != null)
                        exText += ". " + ex.InnerException.Message;

                    sb.AppendFormat("{0}: {1}", ex.ProductId, exText);
                }

                return Error(sb.ToString());
            }
            else
            {
                return Error(exception.Message);
            }
        }

        private JsonResult Error(string text)
        {
            return this.Json(new { @Type = "Error", Text = text }, JsonRequestBehavior.AllowGet);
        }
        private JsonResult Info(string text)
        {
            return this.Json(new { @Type = "Info", Text = text }, JsonRequestBehavior.AllowGet);
        }
        private JsonResult Confirm(string text)
        {
            return this.Json(new { @Type = "Confirm", Text = text }, JsonRequestBehavior.AllowGet);
        }
    }
    #endregion
}
