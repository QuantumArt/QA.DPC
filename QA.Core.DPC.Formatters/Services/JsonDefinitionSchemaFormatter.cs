using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Constants;

namespace QA.Core.DPC.Formatters.Services
{
    public class JsonDefinitionSchemaFormatter : JsonDefinitionSchemaFormatterBase
    {
        public JsonDefinitionSchemaFormatter(IUnityContainer serviceFactory)
            : base(serviceFactory) { }

        protected override bool TreatClassifiersAsBackwardFields
        {
            get { return false; }
        }
    }

    public class JsonDefinitionSchemaClassifiersAsBackwardsFormatter : JsonDefinitionSchemaFormatterBase
    {
        public JsonDefinitionSchemaClassifiersAsBackwardsFormatter(IUnityContainer serviceFactory)
            : base(serviceFactory) { }

        protected override bool TreatClassifiersAsBackwardFields
        {
            get { return true; }
        }
    }

    public abstract class JsonDefinitionSchemaFormatterBase : IFormatter<Content>
    {
        private readonly IUnityContainer _serviceFactory;
        protected abstract bool TreatClassifiersAsBackwardFields { get; }

        public JsonDefinitionSchemaFormatterBase(IUnityContainer serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        #region IFormatter implementation
        public Task<Content> Read(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task Write(Stream stream, Content product)
        {
            var jObject = new JObject();

            jObject["Hash"] = $"{product.GetHashCode()}";
            var fieldService = _serviceFactory.Resolve<IFieldService>();
            var contentService = _serviceFactory.Resolve<IContentService>();

            using (fieldService.CreateQpConnectionScope())
            {
                jObject["Content"] = VisitDefinition(product, new List<Content>(), new Context(fieldService, contentService, TreatClassifiersAsBackwardFields));
            }

            var textWriter = new StreamWriter(stream);
            var writer = new JsonTextWriter(textWriter);
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;
            jObject.WriteTo(writer);

            textWriter.Flush();

            return Task.FromResult(0);
        }


        private static JObject VisitDefinition(Content content, List<Content> visited, Context context)
        {
            if (visited.Any(x => Object.ReferenceEquals(content, x)))
            {
                if (context.visited.Any(x => Object.ReferenceEquals(content, x)))
                    throw new NotSupportedException("Recursive definition are not supported");

                context.visited.Add(content);
            }

            visited.Add(content);

            var fields = GetFieldsFromDB(content, context);


            JObject currentContent = new JObject();
            currentContent["ContentId"] = content.ContentId;

            var bllContent = context.GetOrAdd($"content_{content.ContentId}", () => context.contentService.Read(content.ContentId));

            if (bllContent == null)
            {
                throw new InvalidOperationException($"Content {content.ContentId} is not found in DB.");
            }

            currentContent["ContentName"] = bllContent.NetName;
            currentContent["ContentDisplayName"] = bllContent.Name;

            currentContent["PlainField"] = new JArray(AddFieldData(GetPlainFields(content, context), fields)
                .Select(x => ProcessFieldBase(x)));

            currentContent["EntityField"] = new JArray(AddFieldData(content.Fields
                .Where(ExactType<EntityField>()), fields)
                .Cast<EntityField>()
                .Select(x => ProcessEntityField(x, visited, context)));

            currentContent["BackwardRelationField"] = new JArray(GetBackwardsWithClassifiers(content, context)
                .Cast<BackwardRelationField>()
                .Select(x => ProcessEntityField(x, visited, context)));

            currentContent["VirtualField"] = new JArray(GetVirtualFields(content));

            if (!context.treatClassifiersAsBackwardFields)
            {
                currentContent["ExtensionField"] = new JArray(GetClassifiers(content)
                  .Select(x => ProcessEntityField(x, visited, context)));
            }

            return currentContent;

        }

        private static IEnumerable<JObject> GetVirtualFields(Content content)
        {
            return content.Fields
                .OfType<BaseVirtualField>()
                .Select(MapVirtualField);
        }

        private static JObject MapVirtualField(Field x)
        {
            var obj = JObject.FromObject(x);
            obj.Add("Type", x.GetType().Name);
            return obj;
        }

        private static IEnumerable<Field> AddFieldData(IEnumerable<Field> fields, IEnumerable<Quantumart.QP8.BLL.Field> qpFields)
        {
            foreach (var field in fields)
            {
                AddFieldData(field, qpFields);

                yield return field;
            }
        }

        private static void AddFieldData(Field field, IEnumerable<Quantumart.QP8.BLL.Field> qpFields)
        {
            var qpField = qpFields.FirstOrDefault(x => x.Id == field.FieldId);
            if (qpField == null)
            {
                throw new InvalidDataException($"Definition is incorrect: field {field.FieldId} of content is not present in DB.");
            }

            field.FieldType = qpField.ExactType.ToString();

            if (qpField.ExactType == FieldExactTypes.Numeric)
            {
                if (qpField.IsInteger && qpField.IsLong)
                {
                    field.NumberType = NumberType.Int64;
                }
                else if (qpField.IsInteger && !qpField.IsLong)
                {
                    field.NumberType = NumberType.Int32;
                }
                else if (qpField.IsDecimal)
                {
                    field.NumberType = NumberType.Decimal;
                }
                else if (!qpField.IsDecimal)
                {
                    field.NumberType = NumberType.Double;
                }
                else
                {
                    field.NumberType = NumberType.Unknown;
                }
            }
            else
            {
                field.NumberType = null;
            }

            if (string.IsNullOrEmpty(field.FieldName))
            {
                field.FieldName = qpField.Name;
            }
        }

        private static IEnumerable<ExtensionField> GetClassifiers(Content content)
        {
            return content.Fields
                          .Where(ExactType<ExtensionField>())
                          .Cast<ExtensionField>();
        }

        private static IEnumerable<Field> GetBackwardsWithClassifiers(Content content, Context context)
        {
            var fields = content.Fields
                .Where(ExactType<BackwardRelationField>())
                .OfType<BackwardRelationField>();

            var additionalFields = GetAdditionalBackwardFields(content, context)
                .ToArray();

            return fields.Concat(additionalFields);

        }

        private static IEnumerable<BackwardRelationField> GetAdditionalBackwardFields(Content content, Context context)
        {
            if (!context.treatClassifiersAsBackwardFields)
                yield break;

            var classifiers = GetClassifiers(content);

            var fields = GetFieldsFromDB(content, context);

            foreach (var classifier in classifiers)
            {
                foreach (var relatedContent in classifier.Contents)
                {
                    var aggField = GetFieldsFromDB(relatedContent, context)
                        .Where(x => x.Aggregated)
                        .FirstOrDefault();

                    if (aggField != null)
                    {
                        yield return new BackwardRelationField()
                        {
                            FieldId = aggField.Id,
                            FieldName = $"{classifier.FieldName}_to_content_{relatedContent.ContentId}",
                            Content = relatedContent
                        };
                    }
                }
            }

        }

        private static IEnumerable<Field> GetPlainFields(Content content, Context context)
        {
            var fields = content.Fields.Where(ExactType<PlainField>());
            if (content.LoadAllPlainFields)
            {
                PlainField[] additionalFields = GetAdditional(content, context);

                return fields.Concat(additionalFields);
            }
            return fields;
        }

        private static PlainField[] GetAdditional(Content content, Context context)
        {
            return context.GetOrAdd($"GetAdditional_{content.ContentId}", () =>
            {
                var fields = GetFieldsFromDB(content, context).Where(x =>
                    !x.IsClassifier && x.LinkId == null && x.RelationId == null);

                var additionalFields = fields.Where(x => !content.Fields.Any(y => y.FieldId == x.Id))
                    .Select(x => new PlainField { FieldId = x.Id, FieldName = x.Name })
                    .ToArray();

                return additionalFields;
            });
        }

        private static IEnumerable<Quantumart.QP8.BLL.Field> GetFieldsFromDB(Content content, Context context)
        {
            return context.GetOrAdd($"GetFieldsFromDB_{content.ContentId}", () =>
            {
                return context.fieldService.List(content.ContentId);
            });
        }

        private static JObject ProcessEntityField(EntityField x, List<Content> visited, Context context)
        {
            JObject f = ProcessFieldBase(x);

            f["Content"] = VisitDefinition(x.Content, visited, context);

            return f;
        }

        private static JObject ProcessEntityField(ExtensionField x, List<Content> visited, Context context)
        {
            JObject f = ProcessFieldBase(x);

            f["Contents"] = new JArray(x.Contents.Select(c => VisitDefinition(c, visited, context)));

            return f;
        }

        private static JObject ProcessFieldBase(Field x)
        {
            var f = new JObject();
            f["FieldId"] = x.FieldId;
            f["FieldName"] = x.FieldName;
            f["CustomProperties"] = JObject.FromObject(x.CustomProperties);

            if (!string.IsNullOrEmpty(x.FieldType))
            {
                f["FieldType"] = x.FieldType;
            }
            if (x.NumberType != null)
            {
                f["NumberType"] = x.NumberType.ToString();
            }
            return f;
        }

        private static Func<Field, bool> ExactType<T>()
        {
            return x => x.GetType() == typeof(T);
        }

        class Context
        {
            internal readonly IFieldService fieldService;
            internal readonly IContentService contentService;
            private readonly Dictionary<string, object> cache;
            internal readonly List<Content> visited;
            internal readonly bool treatClassifiersAsBackwardFields;

            public Context(IFieldService fieldService, IContentService contentService, bool treatClassifiersAsBackwardFields)
            {
                this.treatClassifiersAsBackwardFields = treatClassifiersAsBackwardFields;
                this.fieldService = fieldService;
                this.contentService = contentService;
                cache = new Dictionary<string, object>();
                visited = new List<Content>();
            }

            internal T GetOrAdd<T>(string key, Func<T> funct)
            {
                object result;
                if (!cache.TryGetValue(key, out result))
                {
                    result = funct();
                    cache[key] = result;
                }
                return (T)result;

            }
            #endregion
        }


    }
}
