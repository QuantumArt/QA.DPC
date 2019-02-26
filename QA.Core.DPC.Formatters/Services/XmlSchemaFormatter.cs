using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL.Services.API;
using System.Collections.Generic;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Formatters.Services
{
    public class XmlSchemaFormatter : IFormatter<Content>
	{
        #region Constants
        /// <summary>
        /// При превышении количества ссылок на тип он выносится в отдельный complexType
        /// </summary>
        private const int TypeLevel = 1;
        private const string StringType = "string";
		private const string IntegerType = "integer";
		private const string DecimalType = "decimal";
		private const string BooleanType = "boolean";
		private const string DateType = "date";
		private const string DateTimeType = "dateTime";
		private const string UriType = "anyURI";
		#endregion

		#region private fields
		private IFieldService _fieldService;
		private ContentService _contentService;
		#endregion

		public XmlSchemaFormatter(IFieldService fieldService, ServiceFactory factory)
		{
			_fieldService = fieldService;
			_contentService = factory.GetContentService();
		}

		#region IFormatter implementation
		public Task<Content> Read(Stream stream)
		{
			throw new NotImplementedException();
		}

		public async Task Write(Stream stream, Content content)
		{
			var schema = GetSchema(content);
            //schema.Write(stream);


			var schemaSet = new XmlSchemaSet();
			schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
			schemaSet.Add(schema);
			schemaSet.Compile();

			var compiledSchema = schemaSet.Schemas().OfType<XmlSchema>().First();
			compiledSchema.Write(stream);

            await Task.Yield();
		}

		public string Serialize(Content product)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Private methods

		private XmlSchema GetSchema(Content content)
		{
			var schema = new XmlSchema();
			var productInfo = AddElement(schema, "ProductInfo");
			var contentMap = GetContentMap(content, schema, TypeLevel);
			UpdataSchema(contentMap, schema, productInfo, content, null);
			UpdateRegionsTags(productInfo);
			UpdataFileSchema(schema);
			return schema;
		}

		private void UpdataSchema(Dictionary<int, Dictionary<int, string>> contentMap, XmlSchema schema, XmlSchemaGroupBase container, Content content, Field definitionfield, bool isExstension = false, bool isBackward = false)
		{
			var exstensionFields = content.Fields.OfType<ExtensionField>().ToArray();

			if (exstensionFields.Length > 1)
			{
				throw new Exception("Content " + content.ContentId + " has more than one ExtensionField");
			}

			var exstensionField = exstensionFields.FirstOrDefault();

			if (exstensionField != null && exstensionField.ContentMapping.Count() > 1)
			{
                if (exstensionField.ContentMapping.Values.Any(c => c.Fields.Any() || c.LoadAllPlainFields))
                {
                    throw new Exception("Multiple exstensions are not supported for Content " + content.ContentId + " " + string.Join(",", exstensionField.ContentMapping.Select(x => x.Key)));
                }
            }

			var articleFields = _fieldService.List(content.ContentId).ToArray();

			if (articleFields.Any())
			{
				var articleContent = articleFields.First().Content;
				var article = container;

				if (!isExstension)
				{
					if (definitionfield == null)
					{
						var articles = AddElement(container, articleContent.NetPluralName, true);

						if (exstensionField != null && exstensionField.ContentMapping.Any())
						{
							int exstensionContentId = exstensionField.ContentMapping.First().Value.ContentId;
							var exstensionContent = _contentService.Read(exstensionContentId);
							article = AddType(schema, articles, articleContent.NetName, exstensionContent.NetName, exstensionContent.Name);
						}
						else
						{
							article = AddElement(articles, articleContent.NetName, true);
						}
					}
					else
					{
						var field = _fieldService.Read(definitionfield.FieldId);
						bool required = field.Required && !isBackward;
						string fieldName = definitionfield.FieldName ?? field.Name;
                        int key = content.GetHashCode();
                        Dictionary<int, string> keyMap = null;
                        XmlSchemaComplexType type = null;

                        if (contentMap.TryGetValue(content.ContentId, out keyMap))
                        {
                            string typeName = null;

                            if (keyMap.TryGetValue(key, out typeName))
                            {
                                if (typeName == null)
                                {
                                    int typeNameCount = keyMap.Values.Count(v => v != null);
                                    typeName = articleContent.NetName;

                                    if (typeNameCount > 0)
                                    {
                                        typeName += "_" + (typeNameCount + 1);
                                    }

                                    type = RegisterType(schema, typeName, articleContent.Name);
                                    keyMap[key] = typeName;
                                }
                                else
                                {
                                    if (field.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany && !isBackward)
                                    {
                                        AddElement(container, fieldName, new XmlQualifiedName(typeName), required);
                                    }
                                    else
                                    {
                                        var articles = AddSequence(container, fieldName, required);
                                        AddElement(articles, articleContent.NetName, new XmlQualifiedName(typeName), required, true);
                                    }

                                    return;
                                }
                            }

                        }

                        if (field.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany && !isBackward)
                        {
                            article = AddElement(container, fieldName, required, type: type);
                        }
                        else
                        {
                            var articles = AddSequence(container, fieldName, required);
                            article = AddElement(articles, articleContent.NetName, required, true, type: type);
                        }
                    }

					AddElement(article, "Id", IntegerType);
				}

				var plainArticleFields = articleFields.Where(f => f.RelationType == Quantumart.QP8.BLL.RelationType.None);

				var plainFields = from articleField in plainArticleFields
								  join defField in content.Fields.OfType<PlainField>() on articleField.Id equals defField.FieldId into fs
								  from defField in fs.DefaultIfEmpty()
								  select new
								  {
									  Name = defField == null ? articleField.Name : defField.FieldName,
									  Type = GetElementType(articleField),
									  Required = articleField.Required,
									  HasDefinition = defField != null,
									  ArticleField = articleField
								  };

				foreach (var field in plainFields)
				{
					if (content.LoadAllPlainFields || field.HasDefinition)
					{
						if (field.ArticleField.StringEnumItems.Any())
						{
							AddEnumElement(article, field.Name, field.ArticleField, field.Required);
						}
						else
						{
						    AddElement(article, field.Name, field.Type, field.Required);
					    }
				    }
				}

				foreach (var field in content.Fields.OfType<EntityField>())
				{
					bool backward = field is BackwardRelationField;
					UpdataSchema(contentMap, schema, article, field.Content, field, false, backward);
				}

				if (exstensionField != null)
				{
					var exstensionContent = exstensionField.ContentMapping.Values.FirstOrDefault();

					if (exstensionContent != null)
					{
						UpdataSchema(contentMap, schema, article, exstensionContent, exstensionField, true, false);
					}
				}
			}
		}
			
		private void UpdateRegionsTags(XmlSchemaGroupBase schema)
		{
			var regionsTags = AddSequence(schema, "RegionsTags", false);
			var tag = AddElement(regionsTags, "Tag", true, true);
			AddElement(tag, "Title", StringType);
			var regionsValues = AddSequence(tag, "RegionsValues", true);
			var regionsValue = AddElement(regionsValues, "RegionsValue", true, true);
			AddElement(regionsValue, "Value", StringType, true);
			var regions = AddSequence(regionsValue, "Regions", true);
			AddElement(regions, "Region", IntegerType, true, true);
		}

		private void UpdataFileSchema(XmlSchema schema)
		{
			var file = AddType(schema, "File");
			AddElement(file, "Name", StringType);
			AddElement(file, "FileSizeBytes", IntegerType);
			AddElement(file, "AbsoluteUrl", UriType);
		}

		private void GetContentList(ref List<Content> contents, Content content)
		{
			contents.Add(content);

			foreach (var field in content.Fields.OfType<ExtensionField>())
			{
				foreach (var c in field.ContentMapping.Values)
				{
					GetContentList(ref contents, c);
				}
			}

			foreach (var field in content.Fields.OfType<EntityField>())
			{
				GetContentList(ref contents, field.Content);
			}
		}

        private Dictionary<int, Dictionary<int, string>> GetContentMap(Content content, XmlSchema schema, int count = 1)
        {
            var list = new List<Content>();
            GetContentList(ref list, content);

            return list
                .GroupBy(c => c.ContentId)
                .Select(g => new
                {
                    ContentId = g.Key,
                    CodeGroup = g.GroupBy(c => c.GetHashCode())
                })
                .Where(item => item.CodeGroup.Count() > count)
                .ToDictionary(
                    item => item.ContentId,
                    item => item.CodeGroup.ToDictionary(
                        g => g.Key,
                        g => (string)null
                    )
                 );

            //type = RegisterType(schema, articleContent.NetName, articleContent.Name);
        }

        private XmlQualifiedName GetElementType(Quantumart.QP8.BLL.Field field)
		{						
			string typeName = field.Type.Name;

			if (field.IsClassifier)
			{
				return new XmlQualifiedName(StringType, XmlSchema.Namespace);
			}
			if (typeName == "File")
			{
				return new XmlQualifiedName("File");
			}
			if (typeName == "Boolean")
			{
				return new XmlQualifiedName(BooleanType, XmlSchema.Namespace);
			}
			if (typeName == "Numeric")
			{
				if (field.Size == 0)
				{
					return new XmlQualifiedName(IntegerType, XmlSchema.Namespace);
				}
				else
				{
					return new XmlQualifiedName(DecimalType, XmlSchema.Namespace);
				}
			}
			if (typeName == "DateTime")
			{
				return new XmlQualifiedName(DateTimeType, XmlSchema.Namespace);
			}
			if (typeName == "Date")
			{
				return new XmlQualifiedName(DateType, XmlSchema.Namespace);
			}
			else
			{
				return new XmlQualifiedName(StringType, XmlSchema.Namespace);
			}	
		}

		private void AddEnumElement(XmlSchemaGroupBase schema, string name, Quantumart.QP8.BLL.Field field, bool required)
		{
			var values = field.StringEnumItems.Select(itm => itm.Value).ToArray();
			
			var element = new XmlSchemaElement();
			var type = new XmlSchemaSimpleType();
			var restriction = new XmlSchemaSimpleTypeRestriction();
			element.Name = name;
			element.SchemaType = type;
			restriction.BaseTypeName = new XmlQualifiedName(StringType, XmlSchema.Namespace);
			type.Content = restriction;
			schema.Items.Add(element);

			if (!required)
		{
				element.MinOccurs = 0;
				element.MaxOccurs = 1;
		}

			foreach (var item in field.StringEnumItems)
		{
				var option = new XmlSchemaEnumerationFacet();
				AddAnnotation(option, item.Alias);
				option.Value = item.Value;
				restriction.Facets.Add(option);
			}
		}

		void AddAnnotation(XmlSchemaAnnotated element, string value)
		{
			var annotation = new XmlSchemaAnnotation();
			var documentation = new XmlSchemaDocumentation();
			var document = new XmlDocument();
			documentation.Markup = new[] { document.CreateTextNode(value) };
			annotation.Items.Add(documentation);
			element.Annotation = annotation;
		}

		private void AddElement(XmlSchemaGroupBase schema, string name, string type, bool required = true, bool multiple = false)
		{
			AddElement(schema, name, new XmlQualifiedName(type, XmlSchema.Namespace), required, multiple);
		}

		private void AddElement(XmlSchemaGroupBase schema, string name, XmlQualifiedName type, bool required = true, bool multiple = false)
		{
			var element = new XmlSchemaElement();
			element.Name = name;
			element.SchemaTypeName = type;

			if (!required)
			{
				element.MinOccurs = 0;
				element.MaxOccurs = 1;
			}
			else if (multiple)
			{
				element.MinOccurs = 1;
				element.MaxOccursString = "unbounded";
			}
			schema.Items.Add(element);
		}

		private XmlSchemaGroupBase AddElement(XmlSchema schema, string name, bool required = true, bool multiple = false)
		{
			return AddElement(element => schema.Items.Add(element), name, required, multiple);
		}

		private XmlSchemaGroupBase AddElement(XmlSchemaGroupBase schema, string name, bool required = true, bool multiple = false, XmlSchemaComplexType type = null)
		{
			return AddElement(element => schema.Items.Add(element), name, required, multiple, type);
		}

		private XmlSchemaGroupBase AddType(XmlSchema schema, XmlSchemaGroupBase container, string name, string typeName, string description)
		{
			var element = new XmlSchemaElement();
			element.Name = name;
			element.SchemaTypeName = new XmlQualifiedName(typeName);
			var type = new XmlSchemaComplexType();
			type.Name = typeName;
			var all = new XmlSchemaAll();
			schema.Items.Add(type);
			container.Items.Add(element);
			type.Particle = all;
			AddAnnotation(type, description);
			return all;
		}

		private XmlSchemaGroupBase AddType(XmlSchema schema, string typeName)
		{
			var type = new XmlSchemaComplexType();
			type.Name = typeName;
			var all = new XmlSchemaAll();
			schema.Items.Add(type);
			type.Particle = all;
			return all;
		}

		private XmlSchemaComplexType RegisterType(XmlSchema schema, string typeName, string description)
		{
			var type = new XmlSchemaComplexType();
			type.Name = typeName;
			AddAnnotation(type, description);
			schema.Items.Add(type);
			return type;
		}

		private XmlSchemaGroupBase AddSequence(XmlSchemaGroupBase schema, string name, bool required, XmlSchemaComplexType type = null)
		{
			var element = new XmlSchemaElement();

			if (!required)
			{
				element.MinOccurs = 0;
				element.MaxOccurs = 1;
			}

			XmlSchemaComplexType currentType = type == null ? new XmlSchemaComplexType() : type;

			var all = new XmlSchemaSequence();
			
			element.Name = name;
			if (type == null)
			{
				element.SchemaType = currentType;
			}
			else
			{
				element.SchemaTypeName = new XmlQualifiedName(currentType.Name);
			}
			currentType.Particle = all;
			schema.Items.Add(element);
			return all;
		}

		private XmlSchemaGroupBase AddElement(Action<XmlSchemaElement> add, string name, bool required, bool multiple, XmlSchemaComplexType type = null)
		{
			var element = new XmlSchemaElement();

			if (!required)
			{
				element.MinOccurs = 0;
				element.MaxOccursString = multiple ? "unbounded" : "1";
			}
			else if (multiple)
			{
				element.MinOccurs = 1;
				element.MaxOccursString = "unbounded";
			}

			XmlSchemaComplexType currentType = type == null ? new XmlSchemaComplexType() : type;

			var all = new XmlSchemaAll();
			element.Name = name;
			if (type == null)
			{
				element.SchemaType = currentType;
			}
			else
			{
				element.SchemaTypeName = new XmlQualifiedName(currentType.Name);
			}
			currentType.Particle = all;			
			add(element);
			return all;
		}
	
		private void ValidationCallbackOne(object sender, ValidationEventArgs args)
		{
            if (args.Exception != null)
            {
                throw args.Exception;
            }
		}
		#endregion
	}

	public class ContentType
	{
		public string Name { get; set; }
		public int Index { get; set; }
	}
}
