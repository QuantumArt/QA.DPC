using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.Constants;

namespace QA.Core.DPC.API.Search
{
	public class ExtendedQueryConditionMapper : IConditionMapper<ExtendedProductQuery>
	{
		private readonly IFieldService _fieldService;

		public ExtendedQueryConditionMapper(IFieldService fieldService)
		{
			_fieldService = fieldService;
		}
        
        public ConditionBase Map(ExtendedProductQuery source)
		{
            var query = source.Query;

            if (query.Type == JTokenType.String)
            {
                var fieldQuery = new ProductQuery
                {
                    Definition = source.Definition,
                    ExstensionContentIds = source.ExstensionContentIds,
                    Query = query.Value<string>()
                };

                return MapFields(fieldQuery);
            }
            else if(query.Type == JTokenType.Object)
            {

                var name = query.Value<string>("name");
                var subQuery = query["value"];
                
                if (new[] { "AND", "OR" }.Contains(name))
                {
                    if (subQuery.Type == JTokenType.Array)
                    {
                        var condition = new LogicalCondition() { Operation = name };

                        condition.Conditions = subQuery
                            .Select(q => Map(new ExtendedProductQuery
                            {
                                Definition = source.Definition,
                                ExstensionContentIds = source.ExstensionContentIds,
                                Query = q
                            }))
                            .ToArray();

                                       
                        return condition;
                    }
                    else
                    {
                        throw new Exception($"for condition {name} query is not valid: {subQuery}");
                    }
                }
                else if (name == "NOT")
                {
                    if (subQuery.Type == JTokenType.Object || subQuery.Type == JTokenType.String)
                    {                        
                        var condition = new NotCondition(Map(new ExtendedProductQuery
                        {
                            Definition = source.Definition,
                            ExstensionContentIds = source.ExstensionContentIds,
                            Query = subQuery
                        }));

                        return condition;
                    }
                    else
                    {
                        throw new Exception($"for condition {name} query is not valid: {subQuery}");
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new Exception($"query is not valid: {query}");
            }
		}


        public ComparitionCondition MapFields(ProductQuery source)
        {
            string[] queryParams = source.Query.Split(new[] { '=' }, 2);
            string[] fields = queryParams[0].Split('_');
            string stringValue = queryParams[1];
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
                    else
                    {
                        fieldId = field.FieldId;
                        queryField = new QueryField { Name = fieldName };
                        definition = null;
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
