using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation.Resources;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.Extensions.Rules.Remote;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;


namespace QA.ProductCatalog.Validation.Validators
{
    public class ValidationHelper
    {

        public ValidationHelper(RemoteValidationContext model, RemoteValidationResult result, IConnectionProvider provider, ISettingsService settingsService)
        {
            Model = model;
            Result = result;
            _provider = provider;
            _settingsService = settingsService;
        }


        #region private

        private readonly IConnectionProvider _provider;
        private readonly ISettingsService _settingsService;
        #endregion


        public RemoteValidationContext Model { get; }

        public RemoteValidationResult Result { get; }

        public RemotePropertyDefinition GetDefinition(string alias, bool addError = true)
        {
            var def = Model.Definitions.FirstOrDefault(x => x.Alias == alias);
            if (def == null && addError)
            {
                Result.AddErrorMessage(String.Format(RemoteValidationMessages.MissingParam, alias));
            }
            return def;
        }

        public string GetPropertyName(string alias)
        {
            return GetDefinition(alias).PropertyName;
        }

        public T GetValue<T>(string alias)
        {
            var def = GetDefinition(alias);
            if (def == null)
                throw new ApplicationException($"Definition {alias} is not found in the passed context");
            return Model.ProvideValueExact<T>(def);
        }

        public string ConnectionString => _provider.GetConnection();

        public string[] GetListOfParametersNames()
        {
            return new[] {
                        GetSettingStringValue(SettingsTitles.BASE_PARAMETER_FIELD_NAME),
                        GetSettingStringValue(SettingsTitles.BASE_PARAMETER_MODIFIERS_FIELD_NAME),
                        GetSettingStringValue(SettingsTitles.DIRECTION_FIELD_NAME),
                        GetSettingStringValue(SettingsTitles.ZONE_FIELD_NAME)
                    };

        }

        public void IsProductsRegionsWithModifierIntersectionsExist(ArticleService articleService, int productId, int[] regionsIds, int[] productsIds,
                        string regionsName, string modifiersName, int dataOptionId)
        {

            int contentId = GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID);
            var productsList = articleService.List(contentId, productsIds, true).Where(w => !w.Archived).ToArray();
            
           Lookup<int, int[]> productToRegions = (Lookup<int, int[]>)productsList
                 .ToLookup(x => x.Id, x => x.FieldValues.Single(a => a.Field.Name == regionsName).RelatedItems.ToArray());
            Lookup<int, int[]> modifiersToProducts = (Lookup<int, int[]>)productsList
                  .ToLookup(x => x.Id, x => x.FieldValues.Single(a => a.Field.Name == modifiersName).RelatedItems.ToArray());
            var resultIds = new List<int>();
            foreach (var item in productToRegions)
            {
                if (item.Key != productId)
                {
                    if (!modifiersToProducts[item.Key].Any() || !modifiersToProducts[item.Key].Contains(new[] { dataOptionId }))
                    {
                        if (regionsIds.Intersect(item.SelectMany(s => s)).Any())
                        {
                            resultIds.Add(item.Key);
                        }
                    }
                }
            }
            if (resultIds.Any())
            {
                Result.AddModelError(
                    GetPropertyName(regionsName), 
                    string.Format(RemoteValidationMessages.ProductsRepeatingRegions, String.Join(", ", resultIds))
                );
            }
        }

        public bool IsProductsRegionIntersectionsExists(ArticleService articleService, string productsName, int[] productsIds, string regionsName)
        {

            int productTypesContentId = GetSettingValue(SettingsTitles.PRODUCT_TYPES_CONTENT_ID);
            CheckSiteId(productTypesContentId);

            int contentId = GetSettingValue(SettingsTitles.PRODUCTS_CONTENT_ID);

            Dictionary<int, int[]> productToRegions = articleService.List(contentId, productsIds, true)
                  .ToDictionary(x => x.Id, x => x.FieldValues.Single(a => a.Field.Name == regionsName).RelatedItems.ToArray());

            Dictionary<int, HashSet<int>> regionsToProducts = new Dictionary<int, HashSet<int>>();
            foreach (var item in productToRegions)
            {
                foreach (var regionId in item.Value)
                {
                    if (!regionsToProducts.ContainsKey(regionId))
                        regionsToProducts.Add(regionId, new HashSet<int>());

                    if (!regionsToProducts[regionId].Contains(item.Key))
                    {
                        regionsToProducts[regionId].Add(item.Key);
                    }
                }
            }

            var resultIds = productToRegions
                .Where(m => !m.Value.Select(n => regionsToProducts[n].Any(p => p != m.Key))
                .Any(n => n))
                .Select(m => m.Key).
                ToArray();

            if (!resultIds.Any()) return true;

            Result.AddModelError(
                GetPropertyName(productsName), 
                string.Format(RemoteValidationMessages.Products_Different_Regions, String.Join(", ", resultIds))
            );

            return false;
        }

        public void CheckRelationServicesProductsTariff(ArticleService articleService, int contentProductsId,
                                                        string tariffRelationFieldName, int[] relationsIds, string fieldProductParametersName)
        {
            var fieldParentName = GetSettingStringValue(SettingsTitles.FIELD_PARENT_NAME);
            //получение id статей матрицы отношений из услуги на тарифах
            var relationMatrixList = articleService.List(contentProductsId, relationsIds, true).Where(w => !w.Archived)
                                                   .Select(x =>
                                                                x.FieldValues
                                                                .Single(a => a.Field.Name == fieldParentName)
                                                                .RelatedItems.FirstOrDefault())
                                                   .ToArray();
            var productRelationsContentId = GetSettingValue(SettingsTitles.PRODUCT_RELATIONS_CONTENT_ID);
            var relationMatrixElements = articleService.List(productRelationsContentId, relationMatrixList, true);

            foreach (var item in relationMatrixElements)
            {
                GetParametersListFromRelationMatrix(articleService, item, fieldProductParametersName);
            }
        }

        public void GetParametersListFromRelationMatrix(ArticleService articleService, Article article, string parametersFieldName)
        {
            var contentId = article.FieldValues.Where(a => a.Field.Name == parametersFieldName).Select(s => s.Field.ContentId).Single();

            //получение спиcка параметров
            var parametersList =
                article.FieldValues.Single(a => a.Field.Name == parametersFieldName).Value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse).ToArray();

            CheckTariffAreaDuplicateExist(articleService, contentId, parametersList, parametersFieldName);
        }

        public void CheckTariffAreaDuplicateExist(ArticleService articleService, int contentId, int[] parametersList, string parametersFieldName)
        {
            Lookup<int, int[]> tariffAreaLists = (Lookup<int, int[]>)articleService.List(contentId, parametersList, true).Where(w => !w.Archived)
                .ToLookup(s => s.Id, s => s.FieldValues
                    .Where(w => GetListOfParametersNames().Contains(w.Field.Name))
                    .SelectMany(r => r.RelatedItems).ToArray());

            var resultIds = new List<int>();
            for (int i = 0; i < tariffAreaLists.Count; i++)
            {
                for (int j = i + 1; j < tariffAreaLists.Count; j++)
                {
                    var element1 = tariffAreaLists.ElementAtOrDefault(i);
                    var element2 = tariffAreaLists.ElementAtOrDefault(j);
                    if (element1 != null && element2 != null && element1.SelectMany(s => s).Any() && element1.SelectMany(s => s).SequenceEqual(element2.SelectMany(s1 => s1)))
                    {
                        resultIds.Add(element1.Key);
                        resultIds.Add(element2.Key);
                    }
                }
            }
            if (resultIds.Any())
            {
                Result.AddModelError(GetPropertyName(parametersFieldName),
                        string.Format(RemoteValidationMessages.DuplicateTariffsAreas, String.Join(", ", resultIds.Distinct())));
            }
        }

        public void CheckArchivedRelatedEntity(ArticleService articleService, IEnumerable<string> relIdsList, int productId, string idFieldName, int contentId)
        {
            var ids = relIdsList.Where(n => !string.IsNullOrEmpty(n)).Select(int.Parse).Distinct().ToArray();
            var articles = articleService.List(contentId, ids);
            var relIds = articles.Where(w => w.Archived).Select(s => s.Id).ToList();
            if (relIds.Any())
            {
                Result.AddModelError(GetPropertyName(idFieldName),
                            string.Format(RemoteValidationMessages.RelatedEntityIsArchived, productId, String.Join(",", relIds)));
            }
        }

        public void CheckRelationProductsDuplicate(ArticleService articleService, string idFieldName, int contentProductsId, int[] relationsIds)
        {
            var servicesFieldName = GetSettingStringValue(SettingsTitles.SERVICE_FIELD_NAME);
            var relatedProductsIds = articleService.List(contentProductsId, relationsIds, true).Select(x =>
                        x.FieldValues.Single(a => a.Field.Name == servicesFieldName))
                .Select(s => int.Parse(s.Value)).ToArray();
            var duplicateServices = relatedProductsIds.Where(relId => relatedProductsIds.Count(r => r == relId) > 1).Distinct().ToArray();
            if (duplicateServices.Any())
            {
                Result.AddModelError(GetPropertyName(idFieldName),
                string.Format(RemoteValidationMessages.DuplicateRelationsProducts, String.Join(", ", duplicateServices)));
            }
        }

        public void CheckSiteId(int contentId)
        {
            var siteId = (new ContentService(ConnectionString, 1).Read(contentId)).SiteId;
            if (Model.SiteId != siteId)
            {
                throw new ValidationException(RemoteValidationMessages.SiteIdInvalid);
            }

        }

        public int GetSettingValue(SettingsTitles key)
        {
            var valueStr = _settingsService.GetSetting(key);
            int value;
            if (string.IsNullOrEmpty(valueStr) || !int.TryParse(valueStr, out value))
            {
                throw new ValidationException(String.Format(RemoteValidationMessages.Settings_Missing, key));
            }
            return value;
        }


        public string GetSettingStringValue(SettingsTitles key)
        {
            var valueStr = _settingsService.GetSetting(key);
            if (string.IsNullOrEmpty(valueStr))
            {
                throw new ValidationException(String.Format(RemoteValidationMessages.Settings_Missing, key));
            }
            return valueStr;
        }

        public string GetClassifierFieldName(Article product)
        {
            return GetFieldName(product, a => a.Field.ExactType == FieldExactTypes.Classifier);
        }

        public string GetFieldName(Article product, Func<FieldValue, bool> predicate)
        {
            return product.FieldValues.First(predicate).Field.Name;
        }

        public string GetRelatedFieldName(Article product, int contentId)
        {
            return GetFieldName(product, a => a.Field.RelateToContentId.HasValue &&
                                              a.Field.RelateToContentId.Value == contentId);
        }

    }
}