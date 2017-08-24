using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing.Database;
using Article = QA.Core.Models.Entities.Article;

namespace QA.Core.DPC.Loader
{

	public class XmlProductService : IXmlProductService
	{
		public const string RenderTextFieldAsXmlName = "RenderTextFieldAsXml";
	    public const string RenderFileFieldAsImage = "RenderFileFieldAsImage";


        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly string _connectionString;

        public XmlProductService(ILogger logger, ISettingsService settingsService, IConnectionProvider connectionProvider)
        {
            _logger = logger;
            _settingsService = settingsService;
            _connectionString = connectionProvider.GetConnection();
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


		private string ProcessProductWithTags(IArticleFilter filter, params Article[] content)
		{
            using (
                var cs =
                    new QPConnectionScope(_connectionString))
			{
                var ctx = new CallContext(new DBConnector(cs.DbConnection), filter);

				string[] exceptions;
				var doc = ProcessProduct(ctx, out exceptions, content);
				if (doc == null)
					return string.Empty;

				var xml = doc.ToString();

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
					? ObjectFactoryBase.Resolve<IRegionTagReplaceService>().Replace(xml, regionId, exceptions)
					: xml;
			}
		}

		private XDocument ProcessProduct(CallContext ctx, out string[] exceptions, params Article[] articles)
		{
            var doc = new XDocument();

			var products = ConvertProduct(ctx, out exceptions, ctx.Filter.Filter(articles).ToArray());
			doc.Add(products);
			return doc;
		}

        private XObject ConvertProduct(CallContext ctx, out string[] exceptions, params Article[] articles)
		{
			exceptions = null;
			XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";

			var node = new XElement("ProductInfo",
				new XElement("Products", articles.Select(article => Convert(article, ctx, false, true, "Product"))));

			node.Add(new XAttribute(XNamespace.Xmlns + "xsi", ns));

			if (EnableRegionTagReplacement)
			{
				//Получение региональных замен
                var regionIds =
                    articles
					.Select(x => x.GetField("Regions") as MultiArticleField)
					.Where(x => x != null)
					.SelectMany(x => x.Items.Keys).ToArray();

				if (regionIds.Any())
				{
					var tags = GenerateRegionTags(node, regionIds, out exceptions);

					if (tags != null)
						node.Add(tags);
				}

			}
			return node;
		}


        public Article DeserializeProductXml(XDocument productXml, Models.Configuration.Content definition)
        {
            var rootProductElement = productXml.Root.Elements().First().Elements().First();

            var productDeserializer = ObjectFactoryBase.Resolve<IProductDeserializer>();

            return productDeserializer.Deserialize(new XmlProductDataSource(rootProductElement), definition);
        }


		/// <summary>
        ///     Создание элемента xml для региональных замен
		/// </summary>
		/// <param name="produts">Xml-Продукты, для которых генерировать</param>
		/// <param name="regionIds">Идентификаторы регионов, для которых искать значения тегов</param>
		/// <returns></returns>
        private XObject GenerateRegionTags(XObject produts, int[] regionIds, out string[] exceptions)
		{
			var serviceRegionTags = ObjectFactoryBase.Resolve<IRegionTagReplaceService>();
			var textTags = serviceRegionTags.GetTags(produts.ToString());
			var tags = new List<Tuple<int, List<RegionTag>>>();

			foreach (var regId in regionIds)
			{
				var regTags = serviceRegionTags.GetRegionTags(regId).ToList();
				var itemsToRemove = regTags.Where(x => !textTags.Contains(x.Tag)).ToArray();
				regTags.RemoveAll(x => itemsToRemove.Contains(x));
				tags.Add(new Tuple<int, List<RegionTag>>(regId, regTags));
			}

			var node = new XElement("RegionsTags");
			var xmlTags = new List<XElement>();
			var expt = new List<string>();
			foreach (var tag in textTags)
			{
				var tagContent = ConvertRegionTag(tag, tags);
                if (tagContent != null)
                //Тег имеет разные значения для разных регионов => попадает в XML-список замен и должен быть исключен при проведении региональных замен сейчас
				{
					expt.Add(tag);
					xmlTags.Add(new XElement("Tag", new XElement("Title", tag), tagContent));
				}
			}

			exceptions = expt.ToArray();

			if (xmlTags.Count == 0) //Список замен пуст
				return null;

			node.Add(xmlTags);

			return node;
		}

		/// <summary>
        ///     Получение объекта со списком рениональных замен для тега
		/// </summary>
		/// <param name="tag">текст тега</param>
		/// <param name="tags">все региональные замены</param>
		/// <returns></returns>
        private object ConvertRegionTag(string tag, List<Tuple<int, List<RegionTag>>> tags)
		{
			//exclude = false;
			//Выбор листа всех значений данного тега для регионов с записью в RegionId Id региона для которого выбрали (а не для которого оно записано в бд, т.к. может быть записано для родителя)
			var values = tags.Where(x => x.Item2.Any(a => a.Tag == tag))
							 .Select(x => CreateRegionTag(x.Item1, x.Item2.Where(a => a.Tag == tag).FirstOrDefault())).ToList();

			if (values.Count == 0) //Тег не найден в БД
				return null;

			//Перебор тегов, поиск с одинаковыми значениями, выделение их в отдельный
			var results = from v in values
                          group v.RegionId by v.Value
                into g
						  select new { Value = g.Key, Regions = g.ToList() };

            if (results.Count() <= 1) //Тег одинаковый для всех регионов, будез заменен сразу при формировании XML
				return null;

			return new XElement("RegionsValues",
				results.Select(x => new XElement("RegionsValue",
													new XElement("Value", new XCData(x.Value)),
													new XElement("Regions",
														x.Regions.Select(a => new XElement("Region", a)))
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
                    string.Format("Ошибка при генерации xml: ContentName у article не заполнен. ContentId={0} Id={1}",
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

			if (articleField is MultiArticleField && !((MultiArticleField)articleField).GetArticles(ctx.Filter).Any())
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
		    var renderFileAsImage = article.CustomProperties?.ContainsKey(RenderTextFieldAsXmlName) ?? false;

            if (article.PlainFieldType == PlainFieldType.VisualEdit || article.PlainFieldType == PlainFieldType.Textbox)
			{
                if (renderFileAsImage)
				{
					XElement parsedElement;

					try
					{
						parsedElement = XElement.Parse("<r>" + article.Value + "</r>");
					}
					catch (Exception ex)
					{
						throw new Exception(string.Format("Ошибка при парсинге xml из поля {0}: {1}", article.FieldId, ex.Message), ex);
					}

					return parsedElement.Nodes();
				}
					return new XCData(article.Value);
			}
				
			if (article.PlainFieldType == PlainFieldType.DateTime || article.PlainFieldType == PlainFieldType.Date)
			{
				if (!string.IsNullOrWhiteSpace(article.Value))
				{
					DateTime dt;
					if (DateTime.TryParse(article.Value, out dt))
					{
						return dt.ToString("yyyy-MM-ddTHH:mm:ss");
					}
				}
			}
			if (article.PlainFieldType == PlainFieldType.Numeric)
			{
				if (!string.IsNullOrWhiteSpace(article.Value))
				{
					decimal dt;
					if (decimal.TryParse(article.Value, out dt))
					{
						return dt.ToString().Replace(",", ".");
						//при дисериалтзации всегда нужна точка
					}
				}
			}

            var fieldId = article.FieldId.Value;

			if (article.PlainFieldType == PlainFieldType.File && !string.IsNullOrWhiteSpace(article.Value) && !renderFileAsImage)
			{
                var cnn = ctx.Cnn;

                var path = Common.GetFileFromQpFieldPath(cnn, fieldId, article.Value);

                var size = 0;
				try
				{
                    var fi = new FileInfo(path);
					size = (int)fi.Length;
				}
				catch (Exception ex)
				{
					_logger.ErrorException("DBConnector error", ex);
				}

                return new[]
                {
                    new XElement("Name",
                        article.Value.Contains("/")
                            ? article.Value.Substring(article.Value.LastIndexOf("/") + 1)
                            : article.Value),
                  new XElement("FileSizeBytes", size),
                    new XElement("AbsoluteUrl", string.Format(@"{0}/{1}", cnn.GetUrlForFileAttribute(fieldId,
                            true, true), article.Value))
                };
			}
			if ((
                    article.PlainFieldType == PlainFieldType.Image 
                    || article.PlainFieldType == PlainFieldType.DynamicImage 
                    || article.PlainFieldType == PlainFieldType.File && renderFileAsImage
                ) && !string.IsNullOrWhiteSpace(article.Value))
			{
                var cnn = ctx.Cnn;

                return string.Format(@"{0}/{1}", cnn.GetUrlForFileAttribute(fieldId,
						true, true), article.Value);
			}
			return article.Value;
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
				.GetArticles(ctx.Filter)
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
