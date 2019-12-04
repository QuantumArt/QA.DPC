using System;
using System.Diagnostics;
using QA.Core;
using QA.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.Loader;
using QA.Core.DPC.UI;
using QA.Core.Models;
using QA.Core.Models.Extensions;
using QA.Core.Models.Entities;
using QA.Core.Models.Filters.Base;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Admin.WebApp.Binders;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Processors;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QA.Core.Cache;
using QA.Core.DPC.Resources;
using QA.DPC.Core.Helpers;
using Unity;
using QA.ProductCatalog.Admin.WebApp.Filters;
using ActionContext = QA.Core.ProductCatalog.Actions.ActionContext;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{

    public class ProductController : BaseController
    {
        private const string DefaultCultureKey = "_default_culture";
        private readonly Func<string, string, IAction> _getAction;
        private readonly VersionedCacheProviderBase _versionedCacheProvider;
        private readonly Func<string, IArticleFormatter> _getFormatter;
        private readonly IProductLocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly QPHelper _qpHelper;

        public ProductController(Func<string, string, IAction> getAction,
            VersionedCacheProviderBase versionedCacheProvider,
            Func<string, IArticleFormatter> getFormatter, 
            IProductLocalizationService localizationService,
            IProductService productService,
            QPHelper helper
            )
        {
            _getAction = getAction;
            _versionedCacheProvider = versionedCacheProvider;
            _getFormatter = getFormatter;
            _localizationService = localizationService;
            _productService = productService;
            _qpHelper = helper;
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
            ViewBag.HostId = _qpHelper.HostId;
            if (content_item_id <= 0)
            {
                ViewBag.Message = ProductCardStrings.СontentItemIdPositive;
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
                ViewBag.Message = ProductCardStrings.ProductNotFound;
                return View();
            }
            else if (localize)
            {
                cultures = _localizationService.GetCultures();

                if (lang == null)
                {
                    var cookie = Request.Cookies[actionCode + DefaultCultureKey];

                    currentCulture = string.IsNullOrEmpty(cookie) ? cultures[0] : CultureInfo.GetCultureInfo(cookie);
                }
                else
                {
                    currentCulture = CultureInfo.GetCultureInfo(lang);
                    Response.Cookies.Append(actionCode + DefaultCultureKey, lang);
                }

                product = _localizationService.Localize(product, currentCulture);                
            }

            var productLocalized = sw.ElapsedMilliseconds;

            var control = ObjectFactoryBase.Resolve<IProductControlProvider>().GetControlForProduct(product);

            if (control == null)
            {
                ViewBag.Message = ProductCardStrings.ProductIsNotVisual;
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
                    product.AddPlainField("IsRelevant", false, ProductCardStrings.ProductRelevance);
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
                    .AddPlainField("ProductLoaded", productLoadedIn, ProductCardStrings.ProductLoaded)
                    .AddPlainField("ProductLocalized", productLocalized, ProductCardStrings.ProductLocalized)
                    .AddPlainField("ControlLoaded", controlLoadedIn, ProductCardStrings.ControlLoaded)
                    .AddPlainField("ProductCopied", productCopied, ProductCardStrings.ProductCopied)                    
                    .AddPlainField("RelevanceResolved", relevanceResolved, ProductCardStrings.RelevanceResolved)
                    .AddPlainField("RelevanceLoaded", relevanceLoaded, ProductCardStrings.RelevanceLoaded)
                    .AddPlainField("HierarchySorted", hierarchySorted, ProductCardStrings.HierarchySorted));

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
                ViewBag.Message = ProductCardStrings.ProductNotFound;
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
                ViewBag.Message = ProductCardStrings.ProductNotFound;
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

            var urlScheme = Request.Scheme;
            return
                Json(
                    new
                    {
                        Type = "Download",
                        Url = Url.Action("DownloadXml", "Product", new { content_item_id, live, oneTimeKey }, urlScheme)
                    });
        }


        // ReSharper disable once InconsistentNaming
        public ActionResult DownloadXml(int content_item_id, string oneTimeKey, bool live = false)
        {
            if (_versionedCacheProvider.Get(oneTimeKey) == null)
                return new UnauthorizedResult();

            _versionedCacheProvider.Invalidate(oneTimeKey);

            var product = _productService.GetProductById(content_item_id, live);

            if (product == null)
                return NotFound(ProductCardStrings.ProductNotFound);

            var filter = live ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            var xml = ObjectFactoryBase.Resolve<IXmlProductService>().GetProductXml(product, filter);

            return File(Encoding.UTF8.GetBytes(xml),
                "text/xml",
                $"{product.Id}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml");
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
                    ModelState.AddModelError("", ProductCardStrings.TooMuchProducts);
                    return View("Send", model);
                }

                int[] ids;
                try
                {
                    ids = idsList.Select(int.Parse).ToArray();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ProductCardStrings.NotNumberValues + ". " + ex.Message);
                    return View("Send", model);
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var context = new ActionContext { ContentId = 288, ContentItemIds = ids };
                        action.Process(context);
                        model.Message = ProductCardStrings.PublishedAndSendSuccess;
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
            var statusText = ProductCardStrings.NotRelevant;
            if (relevanceInfo.Relevance == ProductRelevance.Missing)
            {
                statusText = ProductCardStrings.Missing;
            }
            else if (relevanceInfo.Relevance == ProductRelevance.Relevant)
            {
                statusText = ProductCardStrings.Relevant;
            }

            article
                .AddPlainField("ConsumerCulture", relevanceInfo.Culture.NativeName, ProductCardStrings.FrontLanguage)
                .AddPlainField("ConsumerStatusText", statusText, ProductCardStrings.FrontStatus)
                .AddPlainField("ConsumerStatusCode", relevanceInfo.Relevance.ToString(), ProductCardStrings.FrontStatusCode)
                .AddPlainField("ConsumerLastPublished", relevanceInfo.LastPublished.ToString(), ProductCardStrings.Published)
                .AddPlainField("ConsumerLastPublishedUserName", relevanceInfo.LastPublishedUserName, ProductCardStrings.PublishedBy);
        }
    }
    #endregion
}
