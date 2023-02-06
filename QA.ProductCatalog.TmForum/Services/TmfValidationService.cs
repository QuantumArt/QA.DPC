using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.Validation.Xaml;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Unity;
using BLL = Quantumart.QP8.BLL;

namespace QA.ProductCatalog.TmForum.Services
{
    public class TmfValidationService : ITmfValidatonService
    {
        private const string StatusTypeId = "125";

        private readonly IContentService _contentService;

        public TmfValidationService(IUnityContainer serviceFactory)
        {
            _contentService = serviceFactory.Resolve<IContentService>("ContentServiceAdapterAlwaysAdmin");
        }

        public void ValidateArticle(BLL.RulesException errors, Article article)
        {
            BLL.Content content = _contentService.Read(article.ContentId);

            ValidatePlainArticle(errors, article, content);

            foreach (ArticleField field in article.Fields.Values)
            {
                FieldExactTypes fieldType = content.Fields.Where(x => x.Name == field.FieldName).Single().ExactType;

                switch (fieldType)
                {
                    case FieldExactTypes.O2MRelation:
                        Article item = ((SingleArticleField)field).Item;
                        if (item != null)
                        {
                            ValidateArticle(errors, item);
                        }
                        break;
                    case FieldExactTypes.M2ORelation:
                    case FieldExactTypes.M2MRelation:
                        Dictionary<int, Article> items = ((MultiArticleField)field).Items;
                        foreach (KeyValuePair<int, Article> relatedArticle in items)
                        {
                            ValidateArticle(errors, relatedArticle.Value);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public static void ValidatePlainArticle(BLL.RulesException errors, Article article, BLL.Content content)
        {
            if (content.DisableXamlValidation || string.IsNullOrWhiteSpace(content.XamlValidation))
            {
                return;
            }

            Dictionary<string, string> values = article.Fields
                .Select(x => x.Value)
                .ToDictionary(x => content.Fields
                        .Where(w => w.Name == x.FieldName)
                        .Select(s => s.FormName)
                        .Single(),
                    v => content.Fields
                        .Where(f => f.Name == v.FieldName)
                        .Select(s => GetContentValue(s.ExactType, v))
                        .Single());

            values[FieldName.ContentItemId] = article.Id.ToString();
            values[FieldName.StatusTypeId] = StatusTypeId;

            try
            {
                Dictionary<string, string> valueState = new(values);
                ValidationParamObject validationParameters = new()
                {
                    Model = values,
                    Validator = content.XamlValidation,
                    AggregatedValidatorList = Array.Empty<string>(),
                    DynamicResource = content.Site.XamlDictionaries,
                    CustomerCode = string.Empty,
                    LocalizeMessages = false,
                    SiteId = content.SiteId,
                    ContentId = content.Id
                };

                ValidationContext validationContext = ValidationServices.ValidateModel(validationParameters);

                if (!validationContext.IsValid)
                {
                    foreach (ValidationError validationError in validationContext.Result.Errors)
                    {
                        errors.Error(
                            validationError.Definition.PropertyName,
                            values[validationError.Definition.PropertyName],
                            validationError.Message);
                    }
                    foreach (string message in validationContext.Messages)
                    {
                        errors.ErrorForModel(message);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(ArticleStrings.CustomValidationFailed, ex.Message);
                errors.ErrorForModel(errorMessage);
            }
        }

        private static string GetContentValue(FieldExactTypes exactType, ArticleField field)
        {
            switch (exactType)
            {
                case FieldExactTypes.Boolean:
                    string fieldValue = ((PlainArticleField)field).Value;
                    return Convert.ToBoolean(!string.IsNullOrWhiteSpace(fieldValue) ? Convert.ToInt32(fieldValue) : false).ToString();
                case FieldExactTypes.O2MRelation:
                    Article item = ((SingleArticleField)field).Item;
                    return item == null ? "" : item.Id.ToString();
                case FieldExactTypes.M2MRelation:
                case FieldExactTypes.M2ORelation:
                    Dictionary<int, Article> items = ((MultiArticleField)field).Items;
                    return items.Count == 0 ? "" : string.Join(" ", items.Keys.ToString());
                default:
                    return ((PlainArticleField)field).Value;
            }
        }
    }
}
