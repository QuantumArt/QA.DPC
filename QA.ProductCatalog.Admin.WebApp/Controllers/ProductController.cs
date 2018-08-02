using System;
using System.Diagnostics;
using QA.Core;
using QA.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
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
using System.Web;
using Unity.Exceptions;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{

    public class ProductController : Controller
    {
        private const string DefaultCultureKey = "_default_culture";
        private readonly Func<string, string, IAction> _getAction;
        private readonly IVersionedCacheProvider _versionedCacheProvider;
        private readonly Func<string, IArticleFormatter> _getFormatter;
        private readonly IProductLocalizationService _localizationService;
        private readonly IProductService _productService;

        public ProductController(Func<string, string, IAction> getAction,
            IVersionedCacheProvider versionedCacheProvider,
            Func<string, IArticleFormatter> getFormatter, 
            IProductLocalizationService localizationService,
            IProductService productService
            )
        {
            _getAction = getAction;
            _versionedCacheProvider = versionedCacheProvider;
            _getFormatter = getFormatter;
            _localizationService = localizationService;
            _productService = productService;
        }

        [RequireCustomAction]
        // ReSharper disable once InconsistentNaming
        public ActionResult Index(int content_item_id,
            string actionCode, 
            bool live = false,
            string[] filters = null,
            bool includeRelevanceInfo = true, 
            bool localize = false,
            string lang = null)
        {    
            if (content_item_id <= 0)
            {
                ViewBag.Message = "Параметры действия некорректны: content_item_id должен быть больше 0.";
                return View();
            }

            var sw = Stopwatch.StartNew();
            var originalProduct = _productService.GetProductById(content_item_id, live);
            var product = originalProduct;
            var cultures = new CultureInfo[0];
            var currentCulture = CultureInfo.InvariantCulture;

            var productLoadedIn = sw.ElapsedMilliseconds;

            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View();
            }
            else if (localize)
            {
                cultures = _localizationService.GetCultures();

                if (lang == null)
                {
                    var cookie = Request.Cookies[actionCode + DefaultCultureKey];

                    currentCulture = cookie != null ? CultureInfo.GetCultureInfo(cookie.Value) : cultures[0];
                }
                else
                {
                    currentCulture = CultureInfo.GetCultureInfo(lang);
                    var cookie = new HttpCookie(actionCode + DefaultCultureKey, lang);
                    Response.AppendCookie(cookie);
                }

                product = _localizationService.Localize(product, currentCulture);                
            }

            var productLocalized = sw.ElapsedMilliseconds;

            var control = ObjectFactoryBase.Resolve<IProductControlProvider>().GetControlForProduct(product);

            if (control == null)
            {
                ViewBag.Message = "Для указанного продукта не задано визуальное представление.";
                return View();
            }

            var controlLoadedIn = sw.ElapsedMilliseconds;
            filters = filters ?? new string[] {};
            var allFilter = new AllFilter(filters.Select(ObjectFactoryBase.Resolve<IArticleFilter>).ToArray());
            product = ArticleFilteredCopier.Copy(product, allFilter);
            if (localize)
            {
                originalProduct = ArticleFilteredCopier.Copy(originalProduct, allFilter);
            }
            
            var productCopied = sw.ElapsedMilliseconds;
            var relevanceResolved = productCopied;

            if (includeRelevanceInfo)
            {                
                var relevanceService = ObjectFactoryBase.Resolve<IProductRelevanceService>();

                relevanceResolved = sw.ElapsedMilliseconds;

                var relevanceItems = relevanceService.GetProductRelevance(localize ? originalProduct : product, live, localize);

                var relevanceField = new MultiArticleField() { FieldName = "Relevance" };                          
                var id = 0;

                product.AddField(relevanceField);

                foreach (var relevanceItem in relevanceItems)
                {
                    var localizedRelevanceArticle = new Article();                    
                    
                    if (Equals(relevanceItem.Culture, CultureInfo.InvariantCulture))
                    {
                        AddRelevanceData(product, relevanceItem);
                    }

                    relevanceField.Items.Add(++id, localizedRelevanceArticle);
                    AddRelevanceData(localizedRelevanceArticle, relevanceItem);
                }

                var isRelevant = relevanceItems.All(r => r.Relevance == ProductRelevance.Relevant);

                if (!isRelevant)
                {
                    product.AddPlainField("IsRelevant", false, "Актуальность продукта");
                }
            }

            var relevanceLoaded = sw.ElapsedMilliseconds;

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

            var hierarchySorted = sw.ElapsedMilliseconds;

            product.AddArticle("Diagnostics",
                new Article()
                    .AddPlainField("ProductLoaded", productLoadedIn, "Продукт загружен")
                    .AddPlainField("ProductLocalized", productLocalized, "Продукт локализован")
                    .AddPlainField("ControlLoaded", controlLoadedIn, "Карточка получена")
                    .AddPlainField("ProductCopied", productCopied, "Продукт скопирован")                    
                    .AddPlainField("RelevanceResolved", relevanceResolved, "Сервис актуальности получен")
                    .AddPlainField("RelevanceLoaded", relevanceLoaded, "Актуальность получена")
                    .AddPlainField("HierarchySorted", hierarchySorted, "Иерархия отсортирована")
                    .AddPlainField("Stopwatch", sw));



            control.DataContext = product;

            return View("Index", new CardModel { Control = control, Cultures = cultures, CurrentCultute = currentCulture });
        }

        [RequireCustomAction]
        // ReSharper disable once InconsistentNaming
        public ActionResult GetXml(int content_item_id, bool live = false)
        {
            var product = _productService.GetProductById(content_item_id, live);
            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View("GetXml");
            }

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            var xml = ObjectFactoryBase.Resolve<IXmlProductService>().GetProductXml(product, filter);

            return View("GetXml", (object)xml);
        }

        // ReSharper disable once InconsistentNaming
        public ActionResult GetProductData(int content_item_id, string formatter, bool live = false, string lang = null, bool simple = false)
        {
            var product = simple
                ? _productService.GetSimpleProductsByIds(new[] {content_item_id}, live).FirstOrDefault()
                : _productService.GetProductById(content_item_id, live);                

            if (product == null)
            {
                ViewBag.Message = "Продукт не найден.";
                return View(formatter);
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
        // ReSharper disable once InconsistentNaming
        public ActionResult GetXmlFromConsumer(int content_item_id, bool live = false)
        {
            var consumerMonitoringServiceFunc = ObjectFactoryBase.Resolve<Func<bool, IConsumerMonitoringService>>();

            var xml = consumerMonitoringServiceFunc(live).GetProduct(content_item_id);

            return View("GetXml", (object)xml);
        }

        [RequireCustomAction]
        // ReSharper disable once InconsistentNaming
        public ActionResult GetXmlDownloadJson(int content_item_id, bool live = false)
        {
            var oneTimeKey = Guid.NewGuid().ToString();

            _versionedCacheProvider.Add(true, oneTimeKey, new string[] { }, TimeSpan.FromMinutes(10));

            var urlScheme = Request.Url?.Scheme;
            return
                Json(
                    new
                    {
                        Type = "Download",
                        Url = Url.Action("DownloadXml", "Product", new { content_item_id, live, oneTimeKey }, urlScheme)
                    },
                    JsonRequestBehavior.AllowGet);
        }


        // ReSharper disable once InconsistentNaming
        public ActionResult DownloadXml(int content_item_id, string oneTimeKey, bool live = false)
        {
            if (_versionedCacheProvider.Get(oneTimeKey) == null)
                return new HttpUnauthorizedResult("Access to the file is restricted because of invalid or missing key.");

            _versionedCacheProvider.Invalidate(oneTimeKey);

            var product = _productService.GetProductById(content_item_id, live);

            if (product == null)
                return HttpNotFound("Продукт не найден.");

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            var xml = ObjectFactoryBase.Resolve<IXmlProductService>().GetProductXml(product, filter);

            return File(Encoding.UTF8.GetBytes(xml),
                "text/xml",
                $"{product.Id}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml");
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

                if (action is IAsyncAction asyncAction)
                {
                    message = await asyncAction.Process(context);
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
            var model = new SendProductModel();
            return View(model);
        }

        [RequireCustomAction]
        [HttpPost]
        public ActionResult Publish(SendProductModel model)
        {
            var action = ObjectFactoryBase.Resolve<PublishAction>();
            if (model != null && ModelState.IsValid)
            {
                var idsList = model.ArticleIds.SplitString(' ', ',', ';', '\n', '\r').Distinct().ToArray();
                if (idsList.Length > 200)
                {
                    ModelState.AddModelError("", @"Слишком много продуктов. Укажите не более 200");
                    return View("Send", model);
                }

                int[] ids;
                try
                {
                    ids = idsList.Select(int.Parse).ToArray();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", @"Указаны нечисловые значения. " + ex.Message);
                    return View("Send", model);
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

            var statusText = relevanceInfo.Relevance == ProductRelevance.Missing
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

            return Json(new {Type = "Error", Text = "Validation", ValidationErrors = errors},
                JsonRequestBehavior.AllowGet);
        }

        private JsonResult Error(ActionException exception)
        {
            if (exception.InnerExceptions.Any())
            {
                var sb = new StringBuilder("Не удалось обработать продукты:");

                foreach (var exception1 in exception.InnerExceptions)
                {
                    var ex = (ProductException) exception1;
                    sb.AppendLine();

                    var exText = ex.Message;

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
            return Json(new { Type = "Error", Text = text }, JsonRequestBehavior.AllowGet);
        }
        private JsonResult Info(string text)
        {
            return Json(new { Type = "Info", Text = text }, JsonRequestBehavior.AllowGet);
        }
    }
    #endregion
}
