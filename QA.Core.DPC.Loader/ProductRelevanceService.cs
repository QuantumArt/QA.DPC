using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.Globalization;
using QA.Core.Logger;
using System.Diagnostics;

namespace QA.Core.DPC.Loader
{

    public class ProductRelevanceService : IProductRelevanceService
    {
		private readonly IArticleFormatter _formatter;	
        private readonly Func<bool, CultureInfo, IConsumerMonitoringService> _consumerMonitoringServiceFunc;
        private readonly IProductLocalizationService _localisationService;
        private readonly ILogger _logger;

        private string GetHash(string data)
        {
            MD5 alg = MD5.Create();
            byte[] inputBytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = alg.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            
            foreach (byte t in hash)
                sb.Append(t.ToString("X2"));
            
            return sb.ToString();
        }

		public ProductRelevanceService(
            IArticleFormatter formatter,
            Func<bool, CultureInfo, IConsumerMonitoringService> consumerMonitoringServiceFunc,
            IProductLocalizationService localisationService,
            ILogger logger
            )
        {
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));

			_formatter = formatter;
            _consumerMonitoringServiceFunc = consumerMonitoringServiceFunc;
            _localisationService = localisationService;
            _logger = logger;
        }

        public RelevanceInfo[] GetProductRelevance(Article product, bool isLive, bool localize)
        {
            return _localisationService.SplitLocalizations(product, localize)
                .Select(e => GetRelevance(e.Value, e.Key, isLive))
                .Where(r => r != null)
                .ToArray();
        }

        private RelevanceInfo GetRelevance(Article localizedProduct, CultureInfo culture, bool isLive)
        {
            var sw = new Stopwatch();
            sw.Start();
            var consumerMonitoringService = _consumerMonitoringServiceFunc(isLive, culture);

            if (consumerMonitoringService == null)
            {
                return null;
            }

            var relevanceInfo = new RelevanceInfo
            {
                Culture = culture,
                LastPublished = null,
                LastPublishedUserName = null,
                Relevance = ProductRelevance.Missing
            };
            
            var consumerProductInfo = consumerMonitoringService.GetProductInfo(localizedProduct.Id);

            sw.Stop();
            _logger.Debug("Relevance: Product info receiving for {1} took {0} sec", sw.Elapsed.TotalSeconds, localizedProduct.Id);

            if (consumerProductInfo != null)
            {
                sw.Reset();
                sw.Start();
                relevanceInfo.LastPublished = consumerProductInfo.Updated;
                relevanceInfo.LastPublishedUserName = consumerProductInfo.LastPublishedUserName;

                var allArticlesNeededToCheck = GetAllProductChildAtriclesToPublish(localizedProduct).ToArray();

                sw.Stop();
                _logger.Debug("Relevance: Status checking for {1} took {0} sec", sw.Elapsed.TotalSeconds, localizedProduct.Id);

                if (isLive && allArticlesNeededToCheck.Any(x => !x.IsPublished))
                {
                    relevanceInfo.Relevance = ProductRelevance.NotRelevant;
                }
                else
                {
                    sw.Reset();
                    sw.Start();

                    string localizedProductData = _formatter.Serialize(localizedProduct, ArticleFilter.DefaultFilter, true);

                    sw.Stop();
                    _logger.Debug("Relevance: Product serializing for {1} took {0} sec", sw.Elapsed.TotalSeconds, localizedProduct.Id);


                    sw.Reset();
                    sw.Start();

                    string localizedProductHash = GetHash(localizedProductData);

                    sw.Stop();
                    _logger.Debug("Relevance: Hash computing for {1} took {0} sec", sw.Elapsed.TotalSeconds, localizedProduct.Id);

                    if (localizedProductHash == consumerProductInfo.Hash)
                    {
                        relevanceInfo.Relevance = ProductRelevance.Relevant;
                    }
                    else
                    {
                        relevanceInfo.Relevance = ProductRelevance.NotRelevant;
                    }
                }
            }

            return relevanceInfo;
        }

        private IEnumerable<Article> GetProductArticlesNeededToPublish(Article product)
        {
            if (product != null && product.PublishingMode == PublishingMode.Publish)
            {
                yield return product;

                foreach (var childArticle in GetAllProductChildAtriclesToPublish(product))
                    yield return childArticle;
            }
        }

        private IEnumerable<Article> GetAllProductChildAtriclesToPublish(Article rootProduct)
        {
            foreach (var singleArticleField in rootProduct.Fields.Values.OfType<SingleArticleField>().Where(x => x.Item != null))
            {
                var articlesToReturn = singleArticleField is ExtensionArticleField
                    ? GetAllProductChildAtriclesToPublish(singleArticleField.Item)
                    : GetProductArticlesNeededToPublish(singleArticleField.Item);

                foreach (var article in articlesToReturn)
                    yield return article;
            }

            foreach (var field in rootProduct.Fields.Values.OfType<MultiArticleField>())
                foreach (var article in field)
                    foreach (var articlesInField in GetProductArticlesNeededToPublish(article))
                        yield return articlesInField;
        }        
    }
}
