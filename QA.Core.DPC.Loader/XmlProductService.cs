using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;
using Article = QA.Core.Models.Entities.Article;
using QA.Core.DPC.QP.Models;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace QA.Core.DPC.Loader
{

    public class XmlProductService : IXmlProductService
	{
		public const string RenderTextFieldAsXmlName = "RenderTextFieldAsXml";
	    public const string RenderFileFieldAsImage = "RenderFileFieldAsImage";
		
		private static Regex _invalidXMLChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);

        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly Customer _customer;
		private readonly IRegionTagReplaceService _regionTagReplaceService;
		private readonly LoaderProperties _loaderProperties;
		private readonly IHttpClientFactory _factory;		

        public XmlProductService(
	        ILogger logger, 
	        ISettingsService settingsService, 
	        IConnectionProvider connectionProvider, 
	        IRegionTagReplaceService regionTagReplaceService, 
	        IOptions<LoaderProperties> loaderProperties,
	        IHttpClientFactory factory	        
	        )
        {
            _logger = logger;
            _settingsService = settingsService;
	        _regionTagReplaceService = regionTagReplaceService;	        
            _customer = connectionProvider.GetCustomer();
            _loaderProperties = loaderProperties.Value;
            _factory = factory;
        }

        public string GetProductXml(Article article, IArticleFilter filter)
		{
			return ProcessProductWithTags(filter, article);
		}

		public string GetSingleXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			return ProcessProductWithTags(filter, articles);
		}

		public string[] GetXmlForProducts(Article[] articles, IArticleFilter filter)
		{
			return articles.Select(x => ProcessProductWithTags(filter, x).ToString()).ToArray();
		}

		private bool _enableRegionTagReplacment = true;
		public bool EnableRegionTagReplacement
		{
			get { return _enableRegionTagReplacment; }
			set
			{
				_enableRegionTagReplacment = value;
			}
		}

		private string XmlToString(XDocument doc)
		{
			var sb = new StringBuilder();
			var xws = new XmlWriterSettings
			{
				CheckCharacters = false, OmitXmlDeclaration = true, Indent = true
			};

			using (var xw = XmlWriter.Create(sb, xws))  
			{  

				doc.WriteTo(xw);  
			}

			return PrepareXml(sb.ToString());
		}
		
		public static string PrepareXml(string text)   
		{
			return _invalidXMLChars.Replace(text, "");
		}   


		private string ProcessProductWithTags(IArticleFilter filter, params Article[] content)
		{
            using (
                var cs =
                    new QPConnectionScope(_customer.ConnectionString, (DatabaseType)_customer.DatabaseType))
			{
                var ctx = new CallContext(new DBConnector(cs.DbConnection), filter);

				string[] exceptions;
				var doc = ProcessProduct(ctx, out exceptions, content);
				if (doc == null)
					return string.Empty;

				var xml = XmlToString(doc);

				//региональные замены
				if (content == null || !content.Any() || content[0] == null)
					return xml;

				//замены региональных тегов (одинаковых для любого региона, не одинаковые в exceptions)
                var regionId = 0;

				foreach (var item in content)
				{
					var regionsField = item.GetField("Regions") as MultiArticleField;

					if (regionsField == null || regionsField.Items.Keys.Count == 0)
						continue;

					regionId = regionsField.Items.Keys.FirstOrDefault();

					break;
				}

				if (regionId == 0)
					return xml;

				return EnableRegionTagReplacement
					? _regionTagReplaceService.Replace(xml, regionId, exceptions)
					: xml;
			}
		}

		private XDocument ProcessProduct(CallContext ctx, out string[] exceptions, params Article[] articles)
		{
            var doc = new XDocument();

			Article[] articles1 = ctx.Filter.Filter(articles).ToArray();
			exceptions = null;
			XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";

			var node = new XElement("ProductInfo",
				new XElement("Products", articles1.Select(article => Convert(article, ctx, false, true, "Product"))));

			node.Add(new XAttribute(XNamespace.Xmlns + "xsi", ns));
			doc.Add(node);

			//Получение региональных замен
			var regionIds =
				articles1
					.Select(x => x.GetField("Regions") as MultiArticleField)
					.Where(x => x != null)
					.SelectMany(x => x.Items.Keys).ToArray();

			if (regionIds.Any())
			{
				var tags = GenerateRegionTags(doc, regionIds, out exceptions);

				if (tags != null)
					node.Add(tags);
			}

			return doc;
		}


		public Article DeserializeProductXml(XDocument productXml, Models.Configuration.Content definition)
        {
            var rootProductElement = productXml.Root?.Elements().First().Elements().First();

            var productDeserializer = ObjectFactoryBase.Resolve<IProductDeserializer>();

            return productDeserializer.Deserialize(new XmlProductDataSource(rootProductElement), definition);
        }


        private XObject GenerateRegionTags(XDocument product, int[] regionIds, out string[] exceptions)
		{
			var tags = _regionTagReplaceService.GetRegionTagValues(XmlToString(product), regionIds);
			
			var node = new XElement("RegionsTags");
			var xmlTags = new List<XElement>();
			var exceptionsList = new List<string>();
			foreach (var tag in tags)
			{
				var tagContent = ConvertRegionTag(tag, regionIds, out bool replace);
				
				if (!replace)
				{
					exceptionsList.Add(tag.Title);					
				}
				
				if (tagContent == null) continue;

				xmlTags.Add(new XElement("Tag", new XElement("Title", tag.Title), tagContent));
			}

			exceptions = exceptionsList.ToArray();

			if (xmlTags.Count == 0) //Список замен пуст
				return null;

			node.Add(xmlTags);

			return node;
		}

        private object ConvertRegionTag(TagWithValues tag, int[] regionIds, out bool replace)
        {

	        if (!tag.Values.Any())
	        {
		        replace = false;
		        return null;
	        }

	        if (EnableRegionTagReplacement && tag.Values.Length == 1 &&
	            tag.Values[0].RegionsId.Length == regionIds.Length)
	        {
		        replace = true;
		        return null;
	        }

	        replace = false;
			return new XElement("RegionsValues",
				tag.Values.Select(x => new XElement("RegionsValue",
													new XElement("Value", new XCData(x.Value)),
													new XElement("Regions",
														x.RegionsId.Select(a => new XElement("Region", a)))
								  )).ToList());
		}


        private object Convert(Article article, CallContext ctx, bool omitType = false, bool addXsi = false, string forceName = null)
		{
			if (article == null)
				return null;
			if (article.ContentId == default(int) && forceName == null)
				return "";
			if (!article.Visible || article.Archived)
				return null;
			if (omitType)
				return GetFields(article, ctx);

			var fs = GetFields(article, ctx);

	        string nodeName = forceName ?? article.ContentName;

			if (string.IsNullOrWhiteSpace(nodeName))
                throw new Exception(
                    string.Format("Error while xml generation: ContentName is not filled in the article. ContentId={0} Id={1}",
                        article.ContentId, article.Id));

			var node = new XElement(nodeName, fs);		

            if (addXsi)
            {
                var typeField = _settingsService.GetSetting(SettingsTitles.PRODUCT_TYPES_FIELD_NAME);

                ArticleField f;

                if (!string.IsNullOrEmpty(typeField))
                {
                    typeField = "Type";
                }

                if (article.Fields.TryGetValue(typeField, out f))
                {
                    XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
                    var sf = f as SingleArticleField;
                    var pf = f as PlainArticleField;

                    if (sf != null && sf.Item != null)
                    {
                        node.Add(new XAttribute(ns + "type", sf.Item.ContentName));
                    }
                    else if (pf != null && !string.IsNullOrEmpty(pf.Value))
                    {
                        node.Add(new XAttribute(ns + "type", pf.Value));
                    }

                }
            }
            return node;
		}

        private IEnumerable<object> GetFields(Article article, CallContext ctx, bool omitId = false)
		{
			var systemFields = GetAttributes(article);
			var fields = (article.Fields.Values
                .Where(
                    x =>
                        (x is PlainArticleField && !string.IsNullOrEmpty(((PlainArticleField)x).Value) ||
                         !(x is PlainArticleField)))
							.Select(x => Convert(x, ctx)).Where(x => x != null));
			if (omitId)
				return fields;

			return systemFields.Union(fields);
		}

        private IEnumerable<object> GetAttributes(Article article)
		{
			yield return new XElement("Id", article.Id);
		}

        private object Convert(ArticleField articleField, CallContext ctx)
		{
			if (articleField is ExtensionArticleField)
			{
				var extField = (ExtensionArticleField)articleField;

				if (extField.GetItem(ctx.Filter) == null)
					return null;

				return ConvertValue(extField, ctx);
			}

			if (articleField is SingleArticleField && ((SingleArticleField)articleField).GetItem(ctx.Filter) == null)
				return null;

			if (articleField is MultiArticleField && !((MultiArticleField)articleField).GetArticlesSorted(ctx.Filter).Any())
				return null;

			return new XElement(articleField.FieldName, (
                articleField is PlainArticleField
                    ? ConvertValue((PlainArticleField)articleField, ctx)
                    : (articleField is SingleArticleField
                        ? ConvertValue((SingleArticleField)articleField, ctx)
                        : (articleField is MultiArticleField ? ConvertValue((MultiArticleField)articleField, ctx) : null))));
		}

        private object ConvertValue(PlainArticleField article, CallContext ctx)
		{
		    var renderFileAsImage = GetBoolProperty(article, RenderFileFieldAsImage);

		    var value = article.Value;
		    if (article.PlainFieldType == PlainFieldType.VisualEdit || article.PlainFieldType == PlainFieldType.Textbox)
			{
                if (GetBoolProperty(article, RenderTextFieldAsXmlName))
				{
					XElement parsedElement;

					try
					{
						parsedElement = XElement.Parse("<r>" + value + "</r>");
					}
					catch (Exception ex)
					{
						throw new Exception(string.Format("Ошибка при парсинге xml из поля {0}: {1}", article.FieldId, ex.Message), ex);
					}

					return parsedElement.Nodes();
				}
                return new XCData(value);
			}
				
			if (article.PlainFieldType == PlainFieldType.DateTime || article.PlainFieldType == PlainFieldType.Date)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					if (DateTime.TryParse(value, out var dt))
					{
						return dt.ToString("yyyy-MM-ddTHH:mm:ss");
					}
				}
			}
			if (article.PlainFieldType == PlainFieldType.Numeric)
			{
				if (!string.IsNullOrWhiteSpace(value))
				{
					if (decimal.TryParse(value, out var dt))
					{
						return dt.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
						//при десериализации всегда нужна точка
					}
				}
			}

            var fieldId = article.FieldId ?? 0;

            if (!string.IsNullOrWhiteSpace(value) && (
	                article.PlainFieldType == PlainFieldType.Image
	                || article.PlainFieldType == PlainFieldType.DynamicImage
	                || article.PlainFieldType == PlainFieldType.File
                ))
            {
	            var cnn = ctx.Cnn;
	            var fieldUrl = cnn.GetUrlForFileAttribute(fieldId, false, false);
	            var shortFieldUrl = cnn.GetUrlForFileAttribute(fieldId, true, true);
	            var valueUrl = $@"{shortFieldUrl}/{value}";
	            
	            if (article.PlainFieldType == PlainFieldType.File && !renderFileAsImage)
	            {
		            var size = Common.GetFileSize(_factory, _loaderProperties, cnn, fieldId, value, $@"{fieldUrl}/{value}");
		            return new[]
		            {
			            new XElement("Name", Common.GetFileNameByUrl(cnn, fieldId, valueUrl)),
			            new XElement("FileSizeBytes", size),
			            new XElement("AbsoluteUrl", valueUrl)
		            };
	            }

	            return valueUrl;
            }

			return value;
		}

        private object ConvertValue(SingleArticleField article, CallContext ctx)
		{
			return Convert(article.GetItem(ctx.Filter), ctx, true);
		}

        private IEnumerable<object> ConvertValue(ExtensionArticleField article, CallContext ctx)
		{
			var item = article.GetItem(ctx.Filter);
			yield return new XElement(article.FieldName, item != null ? item.ContentName : "");
			if (item != null)
			{
				yield return GetFields(item, ctx, true).GetEnumerator();
			}
			//return Convert(article.Item, true);
		}

        private IEnumerable<object> ConvertValue(MultiArticleField article, CallContext ctx)
		{
			return article
				.GetArticlesSorted(ctx.Filter)
				.Select(x => Convert(x, ctx));
		}


		private RegionTag CreateRegionTag(int regId, RegionTag tag)
		{
			return new RegionTag
			{
				Id = tag.Id,
				RegionId = regId,
				Tag = tag.Tag,
				Value = tag.Value
			};
		}

        private bool GetBoolProperty(ArticleField article, string propertyKey)
        {
            object value;

            if (article.CustomProperties != null && article.CustomProperties.TryGetValue(propertyKey, out value))
            {
                if (value is bool)
                {
                    return (bool)value;
                }
            }

            return false;
        }

        private class CallContext
		{
			public readonly DBConnector Cnn;
			public readonly IArticleFilter Filter;

            public CallContext(DBConnector cnn, IArticleFilter filter)
			{
				Cnn = cnn;
				Filter = filter;
			}
		}
	}
}
