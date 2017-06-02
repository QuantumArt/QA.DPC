using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.QP.Services;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;
using Article = QA.Core.Models.Entities.Article;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.Loader
{
	public class JsonProductService : IJsonProductService
	{
		private readonly Quantumart.QP8.BLL.Services.API.FieldService _fieldService;

		private const string ID_PROP_NAME = "Id";
		private readonly ILogger _logger;
		private readonly DBConnector _dbConnector;
		private readonly VirtualFieldPathEvaluator _virtualFieldPathEvaluator;

	    public Article DeserializeProduct(string productJson, Content definition)
	    {
	        var rootArticleDictionary = JsonConvert.DeserializeObject<JObject>(productJson);

            var productDeserializer = ObjectFactoryBase.Resolve<IProductDeserializer>();

            return productDeserializer.Deserialize(new JsonProductDataSource(rootArticleDictionary), definition);
        }

	    internal class PlainFieldFileInfo
		{
			public string Name { get; set; }

			public int FileSizeBytes { get; set; }

			public string AbsoluteUrl { get; set; }
		}

		public JsonProductService(IConnectionProvider connectionProvider, ILogger logger, Quantumart.QP8.BLL.Services.API.FieldService fieldService, VirtualFieldPathEvaluator virtualFieldPathEvaluator, IRegionTagReplaceService regionTagReplaceService)
		{
			_logger = logger;

			_fieldService = fieldService;
            var connectionString = connectionProvider.GetConnection();
            _dbConnector = new DBConnector(connectionString);

			_virtualFieldPathEvaluator = virtualFieldPathEvaluator;

		    _regionTagReplaceService = regionTagReplaceService;
		}

		public string SerializeProduct(Article article, IArticleFilter filter, bool includeRegionTags = false)
		{
		    string productJson = JsonConvert.SerializeObject(ConvertArticle(article, filter), Formatting.Indented);
			var regionField = article.GetField("Regions") as MultiArticleField;

			if (includeRegionTags && regionField != null)
		    {
                int[] regionIds = regionField.Items.Keys.ToArray();

		        TagWithValues[] tags = _regionTagReplaceService.GetRegionTagValues(productJson, regionIds);

		        string tagsJson = JsonConvert.SerializeObject(tags,
					Formatting.Indented,
		            new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

                return $"{{ \"{_productPropertyName}\" : {productJson}, \"{_regionTagsPropertyName}\" : {tagsJson} }}";
		    }
            else
				return $"{{ \"{_productPropertyName}\" : {productJson} }}";
		}

	    private const string _productPropertyName = "product";

	    private const string _regionTagsPropertyName = "regionTags";
        private readonly IRegionTagReplaceService _regionTagReplaceService;

        public JSchema GetSchema(Content definition, bool forList = false, bool includeRegionTags = false)
		{
			var foundFields = new Dictionary<Tuple<Content, string>, Field>();

			var fieldsToIgnore = new List<Tuple<Content, Field>>();

			FillVirtualFieldsInfo(definition, foundFields, fieldsToIgnore);

			JSchema rootSchema = GetSchemaRecursive(definition, forList, new Dictionary<Content, JSchema>(), foundFields, fieldsToIgnore);

		    if (includeRegionTags)
		    {
		        JSchema schemaWithRegionTags = new JSchema {Type = JSchemaType.Object};

                schemaWithRegionTags.Properties.Add(_productPropertyName, rootSchema);

		        JSchema regionTagsSchema = new JSchema {Type = JSchemaType.Array};

                JSchema regionTagSchema = new JSchema { Type = JSchemaType.Object };

		        regionTagSchema.Properties.Add("title", new JSchema {Type = JSchemaType.String});

		        JSchema valuesSchema = new JSchema {Type = JSchemaType.Array};

		        JSchema valueSchema = new JSchema {Type = JSchemaType.Object};

		        valueSchema.Properties.Add("value", new JSchema {Type = JSchemaType.String});

		        JSchema regionIdsSchema = new JSchema {Type = JSchemaType.Array};

		        regionIdsSchema.Items.Add(new JSchema {Type = JSchemaType.Integer});

                valueSchema.Properties.Add("regionsId", regionIdsSchema);

                valueSchema.Required.Add("regionsId");

                valueSchema.Required.Add("value");
                
                valuesSchema.Items.Add(valueSchema);

                regionTagSchema.Properties.Add("values", valuesSchema);

		        regionTagSchema.Required.Add("title");

		        regionTagSchema.Required.Add("values");

                regionTagsSchema.Items.Add(regionTagSchema);

                schemaWithRegionTags.Properties.Add(_regionTagsPropertyName, regionTagsSchema);

		        return schemaWithRegionTags;
		    }
		    else
		        return rootSchema;
		}

		private void FillVirtualFieldsInfo(Content definition, Dictionary<Tuple<Content, string>, Field> foundFields, List<Tuple<Content, Field>> fieldsToDelete, HashSet<Content> parentContents = null)
		{
			if (parentContents == null)
				parentContents = new HashSet<Content>();
			else if (parentContents.Contains(definition))
				return;

			parentContents.Add(definition);

			foreach (var field in definition.Fields)
			{
				if (field is BaseVirtualField)
					ProcessVirtualField((BaseVirtualField)field, definition, foundFields, fieldsToDelete);
				else if (field is EntityField)
					FillVirtualFieldsInfo(((EntityField)field).Content, foundFields, fieldsToDelete, parentContents);
				else if (field is ExtensionField)
				{
					foreach (var content in ((ExtensionField)field).ContentMapping.Values)
						FillVirtualFieldsInfo(content, foundFields, fieldsToDelete, parentContents);
				}
			}
		}

		/// <summary>
		/// рекурсивно обходит Content и заполняет список полей которые надо игнорировать при создании схемы и значения вирутальных полей
		/// </summary>
		/// <param name="virtualField"></param>
		/// <param name="definition"></param>
		/// <param name="foundFields"></param>
		/// <param name="fieldsToDelete"></param>
		private void ProcessVirtualField(BaseVirtualField virtualField, Content definition, Dictionary<Tuple<Content, string>, Field> foundFields, List<Tuple<Content, Field>> fieldsToDelete)
		{
			if (virtualField is VirtualMultiEntityField)
			{
				string path = ((VirtualMultiEntityField)virtualField).Path;

				var keyInFoundFieldsDic = new Tuple<Content, string>(definition, path);

			    if (foundFields.ContainsKey(keyInFoundFieldsDic))
					return;

				bool hasFilter;

				Content parent;

				var foundField = _virtualFieldPathEvaluator.GetFieldByPath(path, definition, out hasFilter, out parent);

				if (!hasFilter)
					fieldsToDelete.Add(new Tuple<Content, Field>(parent, foundField));

				foundFields[keyInFoundFieldsDic] = foundField;

				if (!(foundField is EntityField))
					throw new Exception("В Path VirtualMultiEntityField должны быть только поля EntityField или наследники");

				foreach (var childField in ((VirtualMultiEntityField)virtualField).Fields)
					ProcessVirtualField(childField, ((EntityField)foundField).Content, foundFields, fieldsToDelete);
			}
			else if (virtualField is VirtualEntityField)
				foreach (var childField in ((VirtualEntityField)virtualField).Fields)
					ProcessVirtualField(childField, definition, foundFields, fieldsToDelete);
			else if (virtualField is VirtualField)
			{
				string path = ((VirtualField)virtualField).Path;

				var keyInFoundFieldsDic = new Tuple<Content, string>(definition, path);

				if (foundFields.ContainsKey(keyInFoundFieldsDic))
					return;

				bool hasFilter;

				Content parent;

				var fieldToMove = _virtualFieldPathEvaluator.GetFieldByPath(path, definition, out hasFilter, out parent);

				if (!hasFilter)
					fieldsToDelete.Add(new Tuple<Content, Field>(parent, fieldToMove));

				foundFields[keyInFoundFieldsDic] = fieldToMove;
			}
		}


		private JSchema GetSchemaRecursive(Content definition, bool forList, Dictionary<Content, JSchema> generatedSchemas, Dictionary<Tuple<Content, string>, Field> foundVirtualFields, List<Tuple<Content, Field>> fieldsToIgnore)
		{
			if (generatedSchemas.ContainsKey(definition))
                return generatedSchemas[definition];

			var contentSchema = new JSchema { Type = JSchemaType.Object };

			generatedSchemas[definition] = contentSchema;

            contentSchema.Properties.Add(ID_PROP_NAME, new JSchema {Type = JSchemaType.Integer, Minimum = 0, ExclusiveMinimum = true});

            contentSchema.Required.Add(ID_PROP_NAME);

            var qpFields = _fieldService.List(definition.ContentId).ToArray();

			foreach (var field in definition.Fields.Where(x => !(x is Dictionaries) && (!forList || x is PlainField && ((PlainField)x).ShowInList)))
			{
				if (field.FieldName == null)
					throw new InvalidOperationException(string.Format("FieldName is null: {0}", new { field.FieldId, field.FieldName }));

				if (field is BaseVirtualField)
				{
                    contentSchema.Properties[field.FieldName] = GetVirtualFieldSchema((BaseVirtualField)field, definition, generatedSchemas, fieldsToIgnore, foundVirtualFields);
				}
				else if (!fieldsToIgnore.Contains(new Tuple<Content, Field>(definition, field)))
				{
					var qpField = field is BackwardRelationField ? _fieldService.Read(field.FieldId) : qpFields.SingleOrDefault(x => x.Id == field.FieldId);

					if (qpField == null)
						throw new Exception(string.Format("В definition указано поле id={0} которого нет в контенте id={1}", field.FieldId, definition.ContentId));

				    if (field is ExtensionField)
				        MergeExtensionFieldSchema((ExtensionField)field, contentSchema, generatedSchemas, foundVirtualFields, fieldsToIgnore);
                    else
                        contentSchema.Properties[field.FieldName] = GetFieldSchema(field, qpField, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore);
				}
			}

			if (definition.LoadAllPlainFields && !forList)
			{
				var fieldsToAdd = qpFields
					.Where(x => x.RelationType == Quantumart.QP8.BLL.RelationType.None && definition.Fields.All(y => y.FieldId != x.Id));

				foreach (var qpField in fieldsToAdd.Where(x => !fieldsToIgnore.Any(y => y.Item1.Equals(definition) && y.Item2.FieldId == x.Id)))
                    contentSchema.Properties[qpField.Name] = GetPlainFieldSchema(qpField);
			}

		    if (forList)
		    {
		        var resultSchema = new JSchema
		        {
		            Type = JSchemaType.Array
		        };

		        resultSchema.Items.Add(contentSchema);

		        return resultSchema;
		    }
		    else
		        return contentSchema;
		}

	    private void MergeExtensionFieldSchema(ExtensionField field, JSchema contentSchema, Dictionary<Content, JSchema> generatedSchemas, Dictionary<Tuple<Content, string>, Field> foundVirtualFields, List<Tuple<Content, Field>> fieldsToIgnore)
	    {
	        contentSchema.Properties.Add(field.FieldName, new JSchema {Type = JSchemaType.String});

	        var contentFieldGroups = field.ContentMapping.Values.SelectMany(x => x.Fields).GroupBy(x=>x.FieldName);

	        foreach (var contentFieldGroup in contentFieldGroups)
	        {
	            Field[] groupFields = contentFieldGroup.ToArray();

	            if (groupFields.Length > 1)
	            {
                    JSchema sameNameExtensionFieldsSchema = new JSchema { Type = JSchemaType.Object };

                    foreach (Field extField in groupFields)
                        sameNameExtensionFieldsSchema.OneOf.Add(GetFieldSchema(extField, _fieldService.Read(extField.FieldId), false, generatedSchemas,foundVirtualFields,fieldsToIgnore));
                }
                else
                    contentSchema.Properties[groupFields[0].FieldName] = GetFieldSchema(groupFields[0], _fieldService.Read(groupFields[0].FieldId), false, generatedSchemas, foundVirtualFields, fieldsToIgnore);
            }
	    }

	    private JSchema GetFieldSchema(Field field, Quantumart.QP8.BLL.Field qpField, bool forList, Dictionary<Content, JSchema> generatedSchemas, Dictionary<Tuple<Content, string>, Field> foundVirtualFields, List<Tuple<Content, Field>> fieldsToIgnore)
		{
			if (qpField == null && !(field is BaseVirtualField))
				qpField = _fieldService.Read(field.FieldId);

			if (field is PlainField)
				return GetPlainFieldSchema(qpField);

			if (field is BackwardRelationField)
			{
				var backwardFieldSchema = new JSchema{Type = JSchemaType.Array};

                backwardFieldSchema.Items.Add(GetSchemaRecursive(((BackwardRelationField)field).Content, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore));

			    return backwardFieldSchema;
			}

            if (field is EntityField)
			{
				var fieldContent = ((EntityField)field).Content;

				if (qpField.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany)
					return GetSchemaRecursive(fieldContent, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore);
				else
				{
				    var arrayFieldSchema = new JSchema{Type = JSchemaType.Array};

                    arrayFieldSchema.Items.Add(GetSchemaRecursive(fieldContent, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore));

				    return arrayFieldSchema;
				}
			}
			else
				throw new Exception(string.Format("Поля типа {0} не поддерживается", field.GetType()));
		}



		private JSchema GetVirtualFieldSchema(BaseVirtualField virtualField, Content definition, Dictionary<Content, JSchema> generatedSchemas, List<Tuple<Content, Field>> fieldsToIgnore, Dictionary<Tuple<Content, string>, Field> foundVirtualFields)
		{
			if (virtualField is VirtualMultiEntityField)
			{
				var virtualMultiEntityField = (VirtualMultiEntityField)virtualField;

				var boundField = foundVirtualFields[new Tuple<Content, string>(definition, virtualMultiEntityField.Path)];

				var contentForArrayFields = ((EntityField)boundField).Content;

                var itemSchema = new JSchema { Type = JSchemaType.Object };

				foreach (var childField in virtualMultiEntityField.Fields)
                    itemSchema.Properties[childField.FieldName] = GetVirtualFieldSchema(childField, contentForArrayFields, generatedSchemas, fieldsToIgnore, foundVirtualFields);

			    var virtualMultiEntityFieldSchema = new JSchema {Type = JSchemaType.Array};

                virtualMultiEntityFieldSchema.Items.Add(itemSchema);

                return virtualMultiEntityFieldSchema;
			}
			else if (virtualField is VirtualEntityField)
			{
				var fields = ((VirtualEntityField)virtualField).Fields;

                var virtualEntityFieldSchema = new JSchema{Type = JSchemaType.Object};

                foreach (var childField in fields)
                    virtualEntityFieldSchema.Properties[childField.FieldName] = GetVirtualFieldSchema(childField, definition, generatedSchemas, fieldsToIgnore, foundVirtualFields);

				return virtualEntityFieldSchema;
			}
			else if (virtualField is VirtualField)
			{
				var virtField = (VirtualField)virtualField;

				if (virtField.Converter != null)
					return new JSchema { Type = ConvertTypeToJsType(virtField.Converter.OutputType) };
				else
					return GetFieldSchema(foundVirtualFields[new Tuple<Content, string>(definition, virtField.Path)], null, false,
						generatedSchemas, foundVirtualFields, fieldsToIgnore);
			}
			else
				throw new Exception(string.Format("Поле типа {0} не поддерживается", virtualField.GetType()));
		}

		private JSchemaType ConvertTypeToJsType(Type type)
		{
			if (type == typeof(bool))
				return JSchemaType.Boolean;

			if (type == typeof(string))
				return JSchemaType.String;

			if (type == typeof(int))
				return JSchemaType.Integer;

			throw new Exception("Схема не поддерживает конвертер возвращающий тип " + type);
		}



		private JSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field field)
		{
			var schema = new JSchema();

			switch (field.ExactType)
			{
				case FieldExactTypes.Numeric:
					schema.Type = field.IsInteger ? JSchemaType.Integer : JSchemaType.Number;
					break;

				case FieldExactTypes.File:
					schema.Type = JSchemaType.Object;

                    schema.Required.Add("Name");

                    schema.Required.Add("AbsoluteUrl");

                    schema.Required.Add("FileSizeBytes");

                    schema.Properties["Name"] = new JSchema {Type = JSchemaType.String};

                    schema.Properties["AbsoluteUrl"] = new JSchema {Type = JSchemaType.String};

			        schema.Properties["FileSizeBytes"] = new JSchema
			        {
			            Type = JSchemaType.Integer,
			            Minimum = 0,
			            ExclusiveMinimum = false
			        };
					
					break;

				case FieldExactTypes.Boolean:
					schema.Type = JSchemaType.Boolean;
					break;

				case FieldExactTypes.O2MRelation:
					schema.Type = JSchemaType.Integer;
					break;

				default:
					schema.Type = JSchemaType.String;
					break;
			}

			return schema;
		}

		private object GetPlainArticleFieldValue(PlainArticleField plainArticleField)
		{
			if (plainArticleField.NativeValue == null)
				return null;


			switch (plainArticleField.PlainFieldType)
			{
				case PlainFieldType.File:
					{
						if (string.IsNullOrWhiteSpace(plainArticleField.Value))
							return null;

						string path = Common.GetFileFromQpFieldPath(_dbConnector, plainArticleField.FieldId.Value, plainArticleField.Value);

						int size = 0;

						try
						{
							var fi = new FileInfo(path);

							size = (int)fi.Length;
						}
						catch (Exception ex)
						{
							_logger.ErrorException("DBConnector error", ex);
						}

						return new PlainFieldFileInfo
						{
							Name = plainArticleField.Value.Contains("/")
									? plainArticleField.Value.Substring(plainArticleField.Value.LastIndexOf("/") + 1)
									: plainArticleField.Value,
							FileSizeBytes = size,
							AbsoluteUrl = string.Format(@"{0}/{1}",
								_dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
								plainArticleField.Value)
						};
					}

				case PlainFieldType.Image:
				case PlainFieldType.DynamicImage:
					if (string.IsNullOrWhiteSpace(plainArticleField.Value))
						return null;

					return string.Format(@"{0}/{1}",
						_dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
						plainArticleField.Value);

				case PlainFieldType.Boolean:
					return (decimal)plainArticleField.NativeValue == 1;

				default:
					return plainArticleField.NativeValue;
			}
		}

		private Dictionary<string, object> ConvertArticle(Article article, IArticleFilter filter)
		{
			if (article == null || !article.Visible || article.Archived || !filter.Matches(article))
				return null;

			var dic = new Dictionary<string, object> { { ID_PROP_NAME, article.Id } };

			foreach (var field in article.Fields.Values)
			{
			    if (field is ExtensionArticleField)
			    {
			        MergeExtensionFields(dic, (ExtensionArticleField)field, filter);
			    }
			    else
			    {
                    var value = ConvertField(field, filter);

                    if (value != null && !(value is string && string.IsNullOrEmpty((string)value)))
                        dic[field.FieldName] = value;
                }
			}

			return dic;
		}

        private void MergeExtensionFields(Dictionary<string, object> dic, ExtensionArticleField field, IArticleFilter filter)
        {
            if (field.Item == null)
                return;

            dic[field.FieldName] = field.Item.ContentName;

            foreach (ArticleField childField in field.Item.Fields.Values)
            {
                var value = ConvertField(childField, filter);

                if (value != null && !(value is string && string.IsNullOrEmpty((string)value)))
                    dic[childField.FieldName] = value;
            }
        }

        private object ConvertField(ArticleField field, IArticleFilter filter)
		{
			if (field is PlainArticleField)
				return GetPlainArticleFieldValue((PlainArticleField)field);

			if (field is SingleArticleField)
				return ConvertArticle(((SingleArticleField)field).GetItem(filter), filter);

			if (field is MultiArticleField)
			{
				var articles = ((MultiArticleField)field)
					.GetArticles(filter)
					.Select(x => ConvertArticle(x, filter))
					.ToArray();

				return articles.Length == 0 ? null : articles;
			}

			if (field is VirtualArticleField)
			{
				return ((VirtualArticleField)field).Fields
					.Select(x => new { fieldName = x.FieldName, value = ConvertField(x, filter) })
					.ToDictionary(x => x.fieldName, x => x.value);
			}

			if (field is VirtualMultiArticleField)
				return ((VirtualMultiArticleField)field).VirtualArticles.Select(x => ConvertField(x, filter));

			throw new Exception(string.Format("Поле типа {0} не поддерживается", field.GetType()));
		}
	}
}
