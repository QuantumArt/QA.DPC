using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{

    public class ProductRelevanceService : IProductRelevanceService
    {
		private readonly IArticleFormatter _formatter;
		
	    private readonly Func<bool, IConsumerMonitoringService> _consumerMonitoringServiceFunc;

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

		public ProductRelevanceService(IArticleFormatter formatter, Func<bool, IConsumerMonitoringService> consumerMonitoringServiceFunc)
        {
			if (formatter == null)
				throw new ArgumentNullException("formatter");

			_formatter = formatter;

			_consumerMonitoringServiceFunc = consumerMonitoringServiceFunc;
        }

		public ProductRelevance GetProductRelevance(Article product, bool isLive, out DateTime? lastPublished, out string lastPublishedUserName)
        {
			var productFromConsumer = _consumerMonitoringServiceFunc(isLive).GetProductInfo(product.Id);

			if (productFromConsumer == null)
			{
				lastPublishedUserName = null;

				lastPublished = null;

				return ProductRelevance.Missing;
			}

			lastPublishedUserName = productFromConsumer.LastPublishedUserName;

			lastPublished = productFromConsumer.Updated;

			var allArticlesNeededToCheck = GetAllProductChildAtriclesToPublish(product).ToArray();

            if (isLive && allArticlesNeededToCheck.Any(x => !x.IsPublished))
                return ProductRelevance.NotRelevant;

            DateTime maxProductArticlesModifiedDate = allArticlesNeededToCheck.Max(x => x.Modified);

            if (maxProductArticlesModifiedDate > productFromConsumer.Updated)
                return ProductRelevance.NotRelevant;

			string productData = _formatter.Serialize(product, ArticleFilter.DefaultFilter, true);

			string productHash = GetHash(productData);

            return productHash == productFromConsumer.Hash ? ProductRelevance.Relevant : ProductRelevance.NotRelevant;
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
