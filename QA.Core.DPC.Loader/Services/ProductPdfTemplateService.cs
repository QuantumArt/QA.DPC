using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Services
{
	public class ProductPdfTemplateService : IProductPdfTemplateService
	{
		private readonly IArticleService _articleService;

		
		private readonly ISettingsService _settingsService;

		public ProductPdfTemplateService(IArticleService articleService, ISettingsService settingsService)
		{
			_articleService = articleService;

			_settingsService = settingsService;
		}

		private const string PdfTemplateFieldName = "PdfTemplate";

		private const string ProductTypeFieldName = "Type";

		private const string ProductRegionsFieldName = "Regions";

		public Stream GetTemplate(int productId)
		{
			using (_articleService.CreateQpConnectionScope())
			{
				var productArticle = _articleService.Read(productId);

				var templateFieldVal = productArticle.FieldValues.SingleOrDefault(x => x.Field.Name == PdfTemplateFieldName);

				if (templateFieldVal == null)
					throw new Exception(string.Format("В статье {0} не найдено поле {1}", productId, PdfTemplateFieldName));

				if (!string.IsNullOrEmpty(templateFieldVal.Value))
					return File.OpenRead(templateFieldVal.Field.PathInfo.GetPath(templateFieldVal.Value));

				int sharedTemplatesContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCT_PDF_TEMPLATES_CONTENT_ID));

				//если наступит время когда там будет реально много статей то надо на dbconnector переделать
				var sharedTemplates = _articleService.List(sharedTemplatesContentId, null).ToArray();

				string productType = productArticle.FieldValues.Single(x => x.Field.Name == ProductTypeFieldName).Value;

				int[] regionIds = productArticle.FieldValues.Single(x => x.Field.Name == ProductRegionsFieldName).RelatedItems;

				var sharedTemplate = sharedTemplates.FirstOrDefault(x =>
					x.FieldValues.Single(y => y.Field.Name == ProductTypeFieldName).Value == productType &&
					x.FieldValues.Single(y => y.Field.Name == ProductRegionsFieldName).RelatedItems.Intersect(regionIds).Any());

				if (sharedTemplate != null)
					return GetFileSteamFromArticle(sharedTemplate);

				sharedTemplate = sharedTemplates.FirstOrDefault(x =>
					x.FieldValues.Single(y => y.Field.Name == ProductTypeFieldName).Value == productType &&
					!x.FieldValues.Single(y => y.Field.Name == ProductRegionsFieldName).RelatedItems.Any());

				if (sharedTemplate != null)
					return GetFileSteamFromArticle(sharedTemplate);

				sharedTemplate = sharedTemplates.FirstOrDefault(x =>
					x.FieldValues.Single(y => y.Field.Name == ProductTypeFieldName).ObjectValue == null &&
					!x.FieldValues.Single(y => y.Field.Name == ProductRegionsFieldName).RelatedItems.Any());

				if (sharedTemplate != null)
					return GetFileSteamFromArticle(sharedTemplate);

				return null;
			}
		}

		private Stream GetFileSteamFromArticle(Quantumart.QP8.BLL.Article sharedTemplate)
		{
			var templateFieldVal = sharedTemplate.FieldValues.SingleOrDefault(x => x.Field.Name == PdfTemplateFieldName);

			if (templateFieldVal == null)
				throw new Exception(string.Format("В статье {0} не найдено поле {1}", sharedTemplate.Id, PdfTemplateFieldName));

			if (!string.IsNullOrEmpty(templateFieldVal.Value))
				return File.OpenRead(templateFieldVal.Field.PathInfo.GetPath(templateFieldVal.Value));
			else
				throw new Exception(string.Format("В статье {0} не заполнено поле {1}", sharedTemplate.Id, PdfTemplateFieldName));
		}
	}
}
