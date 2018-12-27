using QA.ProductCatalog.Infrastructure;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.ListTypes;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Validation.Resources;

namespace QA.ProductCatalog.Validation.Validators
{
    public class CommonProductValidator : IRemoteValidator2
    {
        private readonly ISettingsService _service;
        private readonly IConnectionProvider _provider;

        public CommonProductValidator(ISettingsService service, IConnectionProvider provider)
        {
            _provider = provider;
            _service = service;
        }


        public RemoteValidationResult Validate(RemoteValidationContext model, RemoteValidationResult result)
        {
            var helper = new ValidationHelper(model, result, _provider, _service);

            int productTypesContentId = helper.GetSettingValue(SettingsTitles.PRODUCT_TYPES_CONTENT_ID);
            var marketingContentId = helper.GetSettingValue(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID);
            var parametersContentId = helper.GetSettingValue(SettingsTitles.PRODUCTS_PARAMETERS_CONTENT_ID);
            var regionsContentId = helper.GetSettingValue(SettingsTitles.REGIONS_CONTENT_ID);

            var contentId = model.ContentId;
            int productId = helper.GetValue<int>(Constants.FieldId); 

            using (new QPConnectionScope(helper.ConnectionString))
            {
                var articleService = new ArticleService(helper.ConnectionString, 1);

                var product = productId > 0 ? articleService.Read(productId) : articleService.New(contentId);
                var markProductName = helper.GetRelatedFieldName(product, marketingContentId);
                var markProductId = helper.GetValue<int>(markProductName);
                var markProduct = articleService.Read(markProductId);
                var productsName = helper.GetRelatedFieldName(markProduct, contentId);

                var productTypeName = helper.GetClassifierFieldName(product);
                int typeId = helper.GetValue<int>(productTypeName);

                var regionsName = helper.GetRelatedFieldName(product, regionsContentId);

                var parametersName = helper.GetRelatedFieldName(product, parametersContentId);

                int[] regionsIds = helper.GetValue<ListOfInt>(regionsName).ToArray(); 
                int[] parametersIds = helper.GetValue<ListOfInt>(parametersName)?.ToArray(); 


                helper.CheckSiteId(productTypesContentId);

                //Проверка того, что продукт не имеет общих регионов с другими региональными продуктами этого МП
                 var productsIds = markProduct
               .FieldValues.Where(a => a.Field.Name == productsName) 
               .SelectMany(a => a.RelatedItems)
               .ToArray();

                 helper.IsProductsRegionsWithModifierIntersectionsExist(articleService, productId, regionsIds, productsIds,
                     regionsName, Constants.FieldProductModifiers, 0);



                if (productId > 0)
                {
                    //Проверка того, что тарифное направление встречается только один раз в продукте 
                    var contentProductsParametersId = helper.GetSettingValue(SettingsTitles.PRODUCTS_PARAMETERS_CONTENT_ID);
                    if (parametersIds != null)
                    {
                        var parameters = helper.GetParameters(articleService, contentId, parametersIds, "c.BaseParameter is not null");
                        helper.CheckTariffAreaDuplicateExist(parameters, parametersName);
                    }

                    var contentServiceOnTariffId = helper.GetSettingValue(SettingsTitles.SERVICES_ON_TARIFF_CONTENT_ID);
                    var tariffRelationFieldName = helper.GetSettingStringValue(SettingsTitles.TARIFF_RELATION_FIELD_NAME);
                    //Получение id поля Tariffs
                    var fieldService = new FieldService(helper.ConnectionString, 1);
                    var fieldId = fieldService.List(contentServiceOnTariffId).Where(w => w.Name.Equals(tariffRelationFieldName)).Select(s => s.Id).FirstOrDefault();
                    //Получение Id услуг из контента "Услуги на тарифах"
                    var relationsIds = articleService.GetRelatedItems(fieldId, productId, true)?
                                                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(int.Parse)
                                                     .ToArray();
                    if (relationsIds != null && relationsIds.Any())
                    {
                        //Проверка того, что тарифное направление встречается только один раз в связи продуктов
                        helper.CheckRelationServicesProductsTariff(articleService, contentServiceOnTariffId,
                            tariffRelationFieldName, relationsIds, parametersName);

                        //Проверка того, что связь между продуктами встречается единожды
                        helper.CheckRelationProductsDuplicate(articleService, Constants.FieldId, contentServiceOnTariffId, relationsIds);
                    }
                    var relatedIds = new List<string>(); 

                    //Проверка того, что сущность с другой стороны связи не в архиве
                    var productRelationsContentId = helper.GetSettingValue(SettingsTitles.PRODUCT_RELATIONS_CONTENT_ID);
                    var contentService = new ContentService(helper.ConnectionString, 1);
                    
                    //Получение связанных контентов
                    var contents = contentService.Read(productRelationsContentId)
                        .AggregatedContents
                        .Where(w => w.Fields
                                    .Count(a => a.ExactType == FieldExactTypes.O2MRelation && a.RelateToContentId == product.ContentId) >= 2).ToArray();
                    foreach (var con in contents)
                    {
                        //Получение полей, по которым может быть связь
                        var productField = con.Fields
                                                .Where(w => w.ExactType == FieldExactTypes.O2MRelation
                                                            && w.RelateToContentId == product.ContentId
                                                            ).ToArray();
                        foreach (var field in productField)
                        {
                            //Получение ids связанных статей
                            var relatedArticlesId = articleService.GetRelatedItems(field.Id, productId, true)?
                                                      .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(int.Parse)
                                .ToArray();

                            if (relatedArticlesId != null && relatedArticlesId.Any())
                            {
                                var relatedArticles = articleService.List(con.Id, relatedArticlesId, true).Where(w => !w.Archived).Select(s => s).ToList();
                                var relField = productField.Where(f => f.Name != field.Name).Select(s => s).First();
                                var rel = relatedArticles.Select(s => s.FieldValues.First(f => f.Field.Name == relField.Name).Value).ToArray();
                                if (rel.Any())
                                {
                                    relatedIds.AddRange(rel);
                                }
                            }
                        }
                    }
                    helper.CheckArchivedRelatedEntity(articleService, relatedIds, productId, Constants.FieldId, helper.GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID));
                }

                string markProductType =
                    articleService.Read(markProductId)
                        .FieldValues.FirstOrDefault(a => a.Field.Name == productTypeName)?
                        .Value;

                //Проверка, что тип маркетингового продукта и тип продукта -- сопоставимы
                if (!articleService.List(productTypesContentId, null, true).Any(x =>
                                x.FieldValues.FirstOrDefault(a => a.Field.Name == Constants.FieldProductContent)?.Value == typeId.ToString()
                                && x.FieldValues.FirstOrDefault(a => a.Field.Name == Constants.FieldMarkProductContent)?.Value == markProductType))
                {
                    result.AddModelError(helper.GetPropertyName(markProductName), RemoteValidationMessages.SameTypeProductMarketingProduct);
                }
            }
            return result;
        }

    }
}