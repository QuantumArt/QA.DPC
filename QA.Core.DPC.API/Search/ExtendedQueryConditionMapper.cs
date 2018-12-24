using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace QA.Core.DPC.API.Search
{
    public class ExtendedQueryConditionMapper : IConditionMapper<ExtendedProductQuery>
	{
        private const string And = "and";
        private const string Or = "or";
        private const string Not = "not";

        private readonly IFieldService _fieldService;

		public ExtendedQueryConditionMapper(IFieldService fieldService)
		{
			_fieldService = fieldService;
		}

        public ConditionBase Map(ExtendedProductQuery source)
        {
            return MapTree(source);
        }

        public ConditionBase MapTree(ExtendedProductQuery source, string operation = null)
        {
            var query = source.Query;

            if (query.Type == JTokenType.Property)
            {
                var property = (JProperty)query;
                var name = property.Name.Replace("@", string.Empty);

                if (new[] { And, Or, Not }.Contains(name.ToLower()) && property.Value.Type == JTokenType.Object)
                {
                    return MapTree(source.GetQuery(property.Value), name.ToLower());
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    return new LogicalCondition
                    {
                        Operation = Or,
                        Conditions = property.Value.Select(v => v.Value<string>())
                            .Select(v => MapFields(source.GetQuery(new JProperty(name, v))))
                            .ToArray()
                    };
                }                
                else if (property.Value.Type == JTokenType.String || property.Value.Type == JTokenType.Integer || property.Value.Type == JTokenType.Float)
                {
                    return MapFields(source.GetQuery(property));
                }              
                else
                {
                    throw new Exception($"Property {property.Name} is not valid for query : {query}");
                }
            }
            else if (query.Type == JTokenType.Object)
            {
                var obj = (JObject)query;

                if (new[] { And, Or, null }.Contains(operation))
                {
                    var properties = obj.Properties().ToArray();

                    if (properties.Length == 0)
                    {
                        throw new Exception();
                    }
                    else if (properties.Length == 1)
                    {
                        return MapTree(source.GetQuery(properties[0]));
                    }
                    else
                    {
                        return new LogicalCondition
                        {
                            Operation = operation ?? And,
                            Conditions = obj.Properties()
                                .Select(p => MapTree(source.GetQuery(p)))
                                .ToArray()
                        };
                    }
                }
                else if (operation == Not)
                {
                    return new NotCondition(MapTree(source.GetQuery(obj), And));
                }
                else
                {
                    throw new Exception($"Operation {operation} is not valid for query : {query}");
                }
            }
            else
            {
                throw new Exception($"query is not valid: {query}");
            }
        }

        public ComparitionCondition MapFields(ExtendedProductQuery source)
        {
            var property = (JProperty)source.Query;            
            string[] fields = property.Name.Replace("@", string.Empty).Split('.');
            string stringValue = property.Value.Value<string>();
            object value = stringValue;
            var queryFields = new List<QueryField>();
            var definition = source.Definition;
            var exstensionIds = source.ExstensionContentIds;
            int fieldId = 0;

            foreach (string fieldName in fields)
            {
                QueryField queryField = null;

                if (definition == null)
                {
                    throw new Exception("field " + fieldName + " does not match definition");
                }
                else
                {
                    Field field = definition.Fields.FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.CurrentCultureIgnoreCase));

                    if (field is EntityField)
                    {
                        queryField = new QueryField { Name = fieldName };
                        definition = ((EntityField)field).Content;
                    }
                    else if (field is ExtensionField)
                    {
                        var extensionField = (ExtensionField)field;

                        var mapping = extensionField.ContentMapping
                            .Where(m => exstensionIds.Contains(m.Key))
                            .Select(m => new { ContentId = m.Key, Content = m.Value })
                            .FirstOrDefault();


                        if (mapping == null)
                        {
                            throw new Exception("Field " + extensionField.FieldId + ", " + extensionField.FieldName + "does not registered");
                        }
                        else
                        {
                            queryField = new QueryField { Name = fieldName, ContentId = mapping.ContentId };
                            definition = mapping.Content;
                        }
                    }
                    else if (field != null)
                    {
                        fieldId = field.FieldId;
                        queryField = new QueryField { Name = fieldName };
                        definition = null;
                    }
                    else
                    { 
                        throw new Exception($"{fieldName} in not found in condition {property}");
                    }
                }

                queryFields.Add(queryField);
            }

            if (fieldId == 0)
            {
                throw new Exception("Incorrect condition or definition");
            }
            else
            {
                using (_fieldService.CreateQpConnectionScope())
                {
                    var field = _fieldService.Read(fieldId);

                    if (field.ExactType == FieldExactTypes.Numeric)
                    {
                        decimal numericValue;
                        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out numericValue))
                        {
                            value = numericValue;
                        }
                        else
                        {
                            throw new Exception("field " + field.Name + " must be numeric");
                        }
                    }
                    else if (new[] { FieldExactTypes.Time, FieldExactTypes.Date, FieldExactTypes.DateTime }.Contains(field.ExactType))
                    {
                        DateTime dateValue;
                        if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                        {
                            value = dateValue;
                        }
                        else
                        {
                            throw new Exception("field " + field.Name + " must be date");
                        }
                    }
                }

                return new ComparitionCondition(queryFields.ToArray(), value, "="); ;
            }
        }
    }
}
