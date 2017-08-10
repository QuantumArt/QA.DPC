using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace QA.Core.DPC.QP.API.Services
{
    public class TarantoolProductAPIService : IProductSimpleAPIService
    {
        private const string PublishedStatusd = "Published";
        private readonly IProductSimpleService<JToken, JToken> _tarantoolProductService;
        private readonly IIdentityProvider _identityProvider;
        private readonly IStatusProvider _statusProvider;

        public TarantoolProductAPIService(IProductSimpleService<JToken, JToken> tarantoolProductService, IIdentityProvider identityProvider, IStatusProvider statusProvider)
        {
            _tarantoolProductService = tarantoolProductService;
            _identityProvider = identityProvider;
            _statusProvider = statusProvider;
        }

        public Article GetAbsentProduct(int productId, int definitionId, bool isLive, string type)
        {
            var product = new Article
            {
                Id = productId,
                ContentId = 0,
                Visible = true,
                Archived = false,
                ContentName = string.Empty,
                IsPublished = true
            };

            if (!string.IsNullOrEmpty(type))
            {
                product.Fields.Add("Type", new PlainArticleField
                {
                    ContentId = 0,
                    FieldId = 0,
                    FieldName = "Type",
                    Value = type,
                    NativeValue = type,
                    PlainFieldType = PlainFieldType.String
                });
            }

            return product;
        }

        public Article GetProduct(int productId, int definitionId, bool isLive = false)
        {
            var customerCode = _identityProvider.Identity.CustomerCode;
            var productJson = _tarantoolProductService.GetProduct(customerCode, productId, definitionId, isLive);
            var definitionJson = _tarantoolProductService.GetDefinition(customerCode, definitionId);
            var productToken = productJson.SelectToken("data.product");
            var contentToken = definitionJson["Content"];
            var product = GetProduct(productToken, contentToken);
            return product;
        }

        private Article GetProduct(JToken productToken, JToken contentToken)
        {
            int contentId = contentToken.Value<int>("ContentId");
            int statusTypeId = productToken.Value<int>("STATUS_TYPE_ID");
            string statusName = _statusProvider.GetStatusName(contentId, statusTypeId);
            bool isPublished = statusName == PublishedStatusd;

            var product = new Article
            {
                Id = productToken.Value<int>("Id"),
                Archived = productToken.Value<bool>("ARCHIVE"),
                Visible = productToken.Value<bool>("VISIBLE"),
                IsPublished = isPublished,
                Status = statusName,
                Created = productToken.Value<DateTime>("CREATED"),
                Modified = productToken.Value<DateTime>("MODIFIED"),
                ContentId = contentId,
                ContentName = contentToken.Value<string>("ContentName"),
                ContentDisplayName = contentToken.Value<string>("ContentDisplayName"),
            };

            foreach (var fieldToken in contentToken["PlainField"])
            {
                var plainField = new PlainArticleField
                {
                    ContentId = product.ContentId,
                    PlainFieldType = GetFieldType(fieldToken),
                    CustomProperties = new Dictionary<string, object>()
                };

                var numberType = GetNumberType(fieldToken);
                UpdateField(plainField, fieldToken);
                SetValue(plainField, productToken, numberType);
                product.Fields[plainField.FieldName] = plainField;
            }

            foreach (var fieldToken in contentToken["EntityField"])
            {
                var fieldType = GetFieldType(fieldToken);

                if (fieldType == PlainFieldType.M2MRelation || fieldType == PlainFieldType.M2ORelation)
                {
                    var multiField = new MultiArticleField { ContentId = product.ContentId };
                    UpdateField(multiField, fieldToken);
                    var articleTokens = productToken[multiField.FieldName];

                    if (articleTokens != null)
                    {
                        foreach (var articleToken in articleTokens)
                        {
                            var childContentToken = fieldToken["Content"];
                            var article = GetProduct(articleToken, childContentToken);
                            multiField.Items.Add(article.Id, article);
                        }
                    }

                    product.Fields[multiField.FieldName] = multiField;
                }
                else if (fieldType == PlainFieldType.O2MRelation)
                {
                    var singleField = new SingleArticleField { ContentId = product.ContentId };
                    UpdateField(singleField, fieldToken);

                    var articleToken = productToken[singleField.FieldName];

                    if (articleToken != null)
                    {
                        var childContentToken = fieldToken["Content"];
                        var article = GetProduct(articleToken, childContentToken);
                        singleField.Item = article;
                    }

                    product.Fields[singleField.FieldName] = singleField;
                }
                else
                {
                    throw new Exception($"EntityField has unsupportable type {fieldType}");
                }
            }

            foreach (var fieldToken in contentToken["ExtensionField"])
            {
                var extensionField = new ExtensionArticleField { ContentId = product.ContentId };
                UpdateField(extensionField, fieldToken);

                var extensionId = productToken.Value<int>(extensionField.FieldName);
                var childContentToken = fieldToken["Contents"].FirstOrDefault(c => c.Value<int>("ContentId") == extensionId);

                if (childContentToken != null)
                {
                    var article = GetProduct(productToken, childContentToken);
                    article.Id = 0;
                    extensionField.Value = childContentToken.Value<string>("ContentId");
                    extensionField.Item = article;
                    product.Fields[extensionField.FieldName] = extensionField;
                }
            }

            foreach (var fieldToken in contentToken["BackwardRelationField"])
            {
                var backwardField = new BackwardArticleField
                {
                    ContentId = product.ContentId,
                };

                UpdateField(backwardField, fieldToken);
                var articleToken = productToken[backwardField.FieldName];

                if (articleToken != null)
                {
                    foreach (var t in articleToken)
                    {
                        var childContentToken = fieldToken["Content"];
                        var article = GetProduct(t, childContentToken);
                        backwardField.Items.Add(article.Id, article);
                    }
                }

                product.Fields[backwardField.FieldName] = backwardField;
            }

            return product;
        }

        private object SetValue(PlainArticleField field, JToken productToken, NumberType numberType)
        {
            switch (field.PlainFieldType)
            {
                case PlainFieldType.Date:
                case PlainFieldType.DateTime:

                    var dt = productToken.Value<DateTime?>(field.FieldName);

                    if (dt.HasValue)
                    {
                        field.Value = dt.Value.ToString(CultureInfo.InvariantCulture);

                        field.NativeValue = dt;
                    }
                    break;

                case PlainFieldType.Numeric:
                    {
                        object number = null;

                        switch (numberType)
                        {
                            case NumberType.Int32:
                                number = productToken.Value<int?>(field.FieldName);
                                break;
                            case NumberType.Int64:
                                number = productToken.Value<long?>(field.FieldName);
                                break;
                            case NumberType.Decimal:
                                number = productToken.Value<decimal?>(field.FieldName);
                                break;
                            case NumberType.Double:
                                number = productToken.Value<double?>(field.FieldName);
                                break;
                            default:
                                break;
                        };

                        if (number != null)
                        {
                            field.Value = number.ToString();

                            field.NativeValue = number;
                        }
                    }
                    break;

                case PlainFieldType.Boolean:
                    {
                        var number = productToken.Value<bool?>(field.FieldName);

                        if (number != null)
                        {
                            var val = number.Value ? 1m : 0m;
                            field.NativeValue = val;
                            field.Value = val.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    break;
                default:
                    field.Value = productToken.Value<string>(field.FieldName);
                    field.NativeValue = field.Value;
                    break;
            }

            field.Value = field.Value ?? string.Empty;

            return field;
        }

        private NumberType GetNumberType(JToken token)
        {
            var value = token.Value<string>("NumberType");
            NumberType type;

            if (Enum.TryParse<NumberType>(value, out type))
            {
                return type;
            }
            else
            {
                return NumberType.Unknown;
            }
        }

        private PlainFieldType GetFieldType(JToken token)
        {
            var value = token.Value<string>("FieldType");
            PlainFieldType type;

            if (Enum.TryParse<PlainFieldType>(value, out type))
            {
                return type;
            }
            else
            {
                return PlainFieldType.Undefined;
            }
        }

        private void UpdateField(ArticleField field, JToken fieldToken)
        {
            field.FieldId = fieldToken.Value<int>("FieldId");
            field.FieldName = fieldToken.Value<string>("FieldName");
            field.FieldDisplayName = fieldToken.Value<string>("FieldName");
        }    
    }
}