using Newtonsoft.Json;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public sealed class ProductSchema
    {
        public IContentSchema Content { get; set; }

        public Dictionary<string, ContentSchema> Definitions { get; set; }
            = new Dictionary<string, ContentSchema>();
    }

    public interface IContentSchema
    {
        int ContentId { get; set; }
    }

    public sealed class ContentSchema : IContentSchema
    {
        public int ContentId { get; set; }
        public string ContentPath { get; set; }
        public string ContentName { get; set; }
        public string ContentTitle { get; set; }
        public string ContentDescription { get; set; }

        public bool ForExtension { get; set; }

        public Dictionary<string, FieldSchema> Fields { get; set; }
            = new Dictionary<string, FieldSchema>();

        internal ContentSchema ShallowCopy()
        {
            return (ContentSchema)MemberwiseClone();
        }
    }

    public sealed class ContentSchemaIdRef : IContentSchema
    {
        public int ContentId { get; set; }
    }

    public sealed class ContentSchemaJsonRef : IContentSchema
    {
        [JsonIgnore]
        public int ContentId { get; set; }

        [JsonProperty("$ref")]
        public string Ref { get; set; }
    }

    public abstract class FieldSchema
    {
        public int FieldId { get; set; }
        public int FieldOrder { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public string FieldDescription { get; set; }
        public FieldExactTypes FieldType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool ViewInList { get; set; }
        public object DefaultValue { get; set; }

        /// <summary>
        /// Названия классов в иерархии наследования
        /// </summary>
        public List<string> ClassNames
        {
            get
            {
                var types = new List<string>();
                Type type = GetType();
                while (type != typeof(FieldSchema))
                {
                    types.Add(type.Name);
                    type = type.BaseType;
                }
                return types;
            }
        }
    }

    public class PlainFieldSchema : FieldSchema
    {
    }

    public sealed class StringFieldSchema : PlainFieldSchema
    {
        public string RegexPattern { get; set; }
    }

    public sealed class NumericFieldSchema : PlainFieldSchema
    {
        public bool IsInteger { get; set; }
    }

    public sealed class ClassifierFieldSchema : PlainFieldSchema
    {
        public bool Changeable { get; set; }
    }

    public sealed class EnumFieldSchema : PlainFieldSchema
    {
        public bool ShowAsRadioButtons { get; set; }

        public StringEnumItem[] Items { get; set; } = new StringEnumItem[0];
    }

    public abstract class RelationFieldSchema : FieldSchema
    {
        public IContentSchema Content { get; set; }

        public string[] DisplayFieldNames { get; set; } = new string[0];
    }

    public sealed class SingleRelationFieldSchema : RelationFieldSchema
    {
        internal SingleRelationFieldSchema ShallowCopy()
        {
            return (SingleRelationFieldSchema)MemberwiseClone();
        }
    }

    public sealed class MultiRelationFieldSchema : RelationFieldSchema
    {
        public bool IsBackward { get; set; }

        public string OrderByFieldName { get; set; }

        public int? MaxDataListItemCount { get; set; }

        internal MultiRelationFieldSchema ShallowCopy()
        {
            return (MultiRelationFieldSchema)MemberwiseClone();
        }
    }
    
    public sealed class ExtensionFieldSchema : FieldSchema
    {
        public bool Changeable { get; set; }

        public Dictionary<string, IContentSchema> Contents { get; set; }
            = new Dictionary<string, IContentSchema>();

        internal ExtensionFieldSchema ShallowCopy()
        {
            return (ExtensionFieldSchema)MemberwiseClone();
        }
    }
}
