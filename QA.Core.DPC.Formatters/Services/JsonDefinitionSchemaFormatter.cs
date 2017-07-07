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

namespace QA.Core.DPC.Formatters.Services
{
    public class JsonDefinitionSchemaFormatter : JsonDefinitionSchemaFormatterBase
    {
        public JsonDefinitionSchemaFormatter(IUnityContainer container)
            : base(container) { }

        protected override bool TreatClassifiersAsBackwardFields
        {
            get { return false; }
        }
    }

    public class JsonDefinitionSchemaClassifiersAsBackwardsFormatter : JsonDefinitionSchemaFormatterBase
    {
        public JsonDefinitionSchemaClassifiersAsBackwardsFormatter(IUnityContainer container)
            : base(container) { }

        protected override bool TreatClassifiersAsBackwardFields
        {
            get { return true; }
        }
    }

    public abstract class JsonDefinitionSchemaFormatterBase : IFormatter<Content>
    {
        private readonly IUnityContainer _container;
        protected abstract bool TreatClassifiersAsBackwardFields { get; }

        public JsonDefinitionSchemaFormatterBase(IUnityContainer container)
        {
            _container = container;
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
            var fieldService = _container.Resolve<IFieldService>();

            using (fieldService.CreateQpConnectionScope())
            {
                jObject["Content"] = VisitDefinition(product, new List<Content>(), new Context(fieldService, TreatClassifiersAsBackwardFields));
            }

            var textWriter = new StreamWriter(stream);
            var writer = new JsonTextWriter(textWriter);
            writer.Formatting = Newtonsoft.Json.Formatting.Indented;
            jObject.WriteTo(writer);

            textWriter.Flush();

            return Task.FromResult(0);
        }


        private static JObject VisitDefinition(Content content, List<Content> visited, Context fieldService)
        {
            if (visited.Any(x => Object.ReferenceEquals(content, x)))
            {
                if (fieldService.visited.Any(x => Object.ReferenceEquals(content, x)))
                    throw new NotSupportedException("Recursive definition are not supported");

                fieldService.visited.Add(content);
            }

            visited.Add(content);

            var fields = GetFieldsFromDB(content, fieldService);

            JObject currentContent = new JObject();
            currentContent["ContentId"] = content.ContentId;

            if (!string.IsNullOrEmpty(content.ContentName))
            {
                currentContent["ContentName"] = content.ContentName;
            }
            else
            {
                //todo: get contentname from BLL if empty, so hardcode it
                //HARDCODE                                
                currentContent["ContentName"] = $"content_{content.GetHashCode()}_generated";
            }

            currentContent["PlainField"] = new JArray(AddFieldData(GetPlainFields(content, fieldService), fields)
                .Select(x => ProcessFieldBase(x)));

            currentContent["EntityField"] = new JArray(AddFieldData(content.Fields
                .Where(ExactType<EntityField>()), fields)
                .Cast<EntityField>()
                .Select(x => ProcessEntityField(x, visited, fieldService)));

            currentContent["BackwardRelationField"] = new JArray(GetBackwardsWithClassifiers(content, fieldService)
                .Cast<BackwardRelationField>()
                .Select(x => ProcessEntityField(x, visited, fieldService)));

            if (!fieldService.treatClassifiersAsBackwardFields)
            {
                currentContent["ExtensionField"] = new JArray(GetClassifiers(content)
                  .Select(x => ProcessEntityField(x, visited, fieldService)));
            }

            return currentContent;

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
            field.NumberType = qpField.IsLong ? NumberType.Int64 : (qpField.IsInteger ? NumberType.Int32 : (qpField.IsDecimal ? NumberType.Double : (NumberType?)null));
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
            private readonly Dictionary<string, object> cache;
            internal readonly List<Content> visited;
            internal readonly bool treatClassifiersAsBackwardFields;

            public Context(IFieldService fieldService)
            {
                this.fieldService = fieldService;
                cache = new Dictionary<string, object>();
                visited = new List<Content>();
            }

            public Context(IFieldService fieldService, bool treatClassifiersAsBackwardFields) : this(fieldService)
            {
                this.treatClassifiersAsBackwardFields = treatClassifiersAsBackwardFields;
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
