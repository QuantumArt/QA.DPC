using Newtonsoft.Json;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using System.Collections.Generic;

namespace QA.Core.DPC.Loader.Editor
{
    public class ProductSchema
    {
        public IContentSchema Content { get; set; }

        public Dictionary<string, ContentSchema> Definitions { get; set; }
            = new Dictionary<string, ContentSchema>();
    }

    public interface IContentSchema
    {
        int ContentId { get; set; }
    }

    public class ContentSchema : IContentSchema
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

    public class ContentSchemaIdRef : IContentSchema
    {
        public int ContentId { get; set; }
    }

    public class ContentSchemaJsonRef : IContentSchema
    {
        [JsonIgnore]
        public int ContentId { get; set; }

        [JsonProperty("$ref")]
        public string Ref { get; set; }
    }
    
    public class FieldSchema
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldTitle { get; set; }
        public string FieldDescription { get; set; }
        public int FieldOrder { get; set; }
        public FieldExactTypes FieldType { get; set; }

        public bool IsRequired { get; set; }
    }
    
    public interface IRelationFieldSchema
    {
        bool IsBackward { get; }

        IContentSchema Content { get; set; }
    }

    public class RelationFieldSchema : FieldSchema, IRelationFieldSchema
    {
        public bool IsBackward => false;

        public IContentSchema Content { get; set; }

        internal RelationFieldSchema ShallowCopy()
        {
            return (RelationFieldSchema)MemberwiseClone();
        }
    }

    public class BackwardFieldSchema : FieldSchema, IRelationFieldSchema
    {
        public bool IsBackward => true;

        public IContentSchema Content { get; set; }

        internal BackwardFieldSchema ShallowCopy()
        {
            return (BackwardFieldSchema)MemberwiseClone();
        }
    }

    public class ExtensionFieldSchema : FieldSchema
    {
        public Dictionary<string, IContentSchema> Contents { get; set; }
            = new Dictionary<string, IContentSchema>();

        internal ExtensionFieldSchema ShallowCopy()
        {
            return (ExtensionFieldSchema)MemberwiseClone();
        }
    }

    public class StringFieldSchema : FieldSchema
    {
        public string RegexPattern { get; set; }
    }

    public class NumericFieldSchema : FieldSchema
    {
        public bool IsInteger { get; set; }
    }

    public class EnumFieldSchema : FieldSchema
    {
        public bool ShowAsRadioButtons { get; set; }

        public StringEnumItem[] Items { get; set; } = new StringEnumItem[0];
    }
}
