using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml.Extensions.Rules.Remote;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.Resources;

namespace QA.ProductCatalog.Validation.Validators
{
    public class ValidationHelper
    {

        public static string ResourceClass = nameof(RemoteValidationMessages);

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
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.MissingParam),
                    Parameters = new object[] {alias}
                };
                Result.AddErrorMessage(JsonConvert.SerializeObject(result));
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
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.DefinitionNotFound), 
                    Parameters = new object[] { alias }
                };
                throw new ValidationException(result);
            }
                throw new ApplicationException($"Definition {alias} is not found in the passed context");
            return Model.ProvideValueExact<T>(def);
        }

        public Customer Customer => _provider.GetCustomer();

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
                var message = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.ProductsRepeatingRegions),
                    Parameters = new object[] {String.Join(", ", resultIds)}
                };
                
                Result.AddModelError(GetPropertyName(regionsName), JsonConvert.SerializeObject(message));
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

            var result = new ActionTaskResultMessage()
            {
                ResourceClass = ResourceClass,
                ResourceName = nameof(RemoteValidationMessages.Products_Different_Regions), 
                Parameters = new object[] {string.Join(", ", resultIds)}
            };

            Result.AddModelError(GetPropertyName(productsName), JsonConvert.SerializeObject(result));

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
            var relationMatrixElements = articleService.List(productRelationsContentId, relationMatrixList, true).ToArray();
            if (relationMatrixElements.Any())
            {
                var contentId = relationMatrixElements.First()
                    .FieldValues
                    .Where(a => a.Field.Name == fieldProductParametersName)
                    .Select(s => s.Field.RelateToContentId ?? 0)
                    .Single();

                var matrixElems = relationMatrixElements.ToDictionary(
                    k => k.Id,
                    v => v.FieldValues
                        .Where(a => a.Field.Name == fieldProductParametersName)
                        .SelectMany(r => r.RelatedItems)
                        .ToArray()
                );
                var matrixElemsParamsIds = matrixElems.SelectMany(n => n.Value).Distinct().ToArray();
                var parametersDict = GetParameters(articleService, contentId, matrixElemsParamsIds, "c.BaseParameter is not null")
                    .ToDictionary(n => n.Id, m => m);

                foreach (var item in matrixElems)
                {
                    var parameters = item.Value
                        .Select(n => parametersDict.ContainsKey(n) ? parametersDict[n] : null)
                        .Where(n => n != null)
                        .ToArray();
                    if (parameters.Any())
                    {
                        CheckTariffAreaDuplicateExist(parameters, fieldProductParametersName, true);                       
                    }
                }
            }
        }


        public int[] GetParameterTariffDirectionIds(Article article, HashSet<string> names)
        {
            return article.FieldValues
                .Where(w => names.Contains(w.Field.Name))
                .SelectMany(r => r.RelatedItems)
                .OrderBy(n => n)
                .ToArray();
        }

        public IEnumerable<Article> GetParameters(ArticleService articleService, int contentId, int[] parametersList, string filter = "")
        {
            return articleService.List(contentId, parametersList, true, filter);
        }
        
        public void CheckTariffAreaDuplicateExist(IEnumerable<Article> parameters, string parametersFieldName, bool isMatrix = false)
        {
            var resultIds = new List<int>();
            var names = new HashSet<string>(GetListOfParametersNames());
            var tariffDirections = parameters.ToDictionary(
                    k => k.Id, 
                    s => string.Join(",", GetParameterTariffDirectionIds(s, names))
                );
            
            var checkedDirections = new Dictionary<string, int>();
            foreach (var tariffDirection in tariffDirections)
            {
                if (!checkedDirections.ContainsKey(tariffDirection.Value))
                {
                    checkedDirections.Add(tariffDirection.Value, tariffDirection.Key);
                }
                else
                {
                    resultIds.Add(tariffDirection.Key);
                    resultIds.Add(checkedDirections[tariffDirection.Value]);
                }
                
            }
            
            if (resultIds.Any())
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = isMatrix ? nameof(RemoteValidationMessages.DuplicateTariffsAreasMatrix) : nameof(RemoteValidationMessages.DuplicateTariffsAreas), 
                    Parameters = new object[] {string.Join(", ", resultIds.Distinct())}
                };

                Result.AddModelError(GetPropertyName(parametersFieldName), JsonConvert.SerializeObject(result));
            }
        }

        public void CheckArchivedRelatedEntity(ArticleService articleService, IEnumerable<string> relIdsList, int productId, string idFieldName, int contentId)
        {
            var ids = relIdsList.Where(n => !string.IsNullOrEmpty(n)).Select(int.Parse).Distinct().ToArray();
            var articles = articleService.List(contentId, ids);
            var relIds = articles.Where(w => w.Archived).Select(s => s.Id).ToList();
            if (relIds.Any())
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.RelatedEntityIsArchived), 
                    Parameters = new object[] {string.Join(",", relIds.Distinct())}
                };
                
                Result.AddModelError(GetPropertyName(idFieldName), JsonConvert.SerializeObject(result));
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
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.DuplicateRelationsProducts), 
                    Parameters = new object[] {string.Join(",", duplicateServices)}
                };

                Result.AddModelError(GetPropertyName(idFieldName), JsonConvert.SerializeObject(result));
            }
        }

        public void CheckSiteId(int contentId)
        {
            var siteId = new ContentService(Customer.ConnectionString, 1).Read(contentId).SiteId;
            if (Model.SiteId != siteId)
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.SiteIdInvalid), 
                    Parameters = new object[] { siteId, Model.SiteId }
                };
                throw new ValidationException(result);
            }

        }

        public int GetSettingValue(SettingsTitles key)
        {
            var valueStr = _settingsService.GetSetting(key);
            int value;
            if (string.IsNullOrEmpty(valueStr) || !int.TryParse(valueStr, out value))
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.Settings_Missing), 
                    Parameters = new object[] { key }
                };
                throw new ValidationException(result);
            }
            return value;
        }


        public string GetSettingStringValue(SettingsTitles key)
        {
            var valueStr = _settingsService.GetSetting(key);
            if (string.IsNullOrEmpty(valueStr))
            {
                var result = new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = nameof(RemoteValidationMessages.Settings_Missing), 
                    Parameters = new object[] { key }
                };
                throw new ValidationException(result);
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