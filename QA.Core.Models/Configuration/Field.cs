using QA.Core.Models.Tools;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QA.Core.DPC.Resources;


namespace QA.Core.Models.Configuration
{
    public abstract class Field
    {
        /// <summary>
        /// специфичные для конкретных сериализаторов свойства
        /// лоадер их не использует
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; private set; }

        protected Field()
        {
            CustomProperties = new Dictionary<string, object>();
        }

        [Display(Name = "FieldName", ResourceType = typeof(ControlStrings))]
        public virtual string FieldName { get; set; }

        [Display(Name = "FieldNameForCard", ResourceType = typeof(ControlStrings))]

        [DefaultValue(null)]
        public virtual string FieldTitle { get; set; }

        public virtual int FieldId { get; set; }

        [DefaultValue(null)]
        public virtual string FieldType { get; set; }

        [DefaultValue(null)]
        public NumberType? NumberType { get; set; }
        
        public Field ShallowCopy()
        {
            var field = (Field)MemberwiseClone();

            ShallowCopyMembers(field);

            return field;
        }

        protected virtual void ShallowCopyMembers(Field field)
        {
            field.CustomProperties = CustomProperties?
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public Field DeepCopy()
        {
            return DeepCopy(new ReferenceDictionary<object, object>());
        }

        internal Field DeepCopy(ReferenceDictionary<object, object> visited)
        {
            if (visited.TryGetValue(this, out object value))
            {
                return (Field)value;
            }

            var field = (Field)MemberwiseClone();

            visited[this] = field;

            DeepCopyMembers(field, visited);

            return field;
        }

        protected virtual void DeepCopyMembers(Field field, ReferenceDictionary<object, object> visited)
        {
            field.CustomProperties = CustomProperties?
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        internal virtual void FillChildContents(List<Content> parents)
		{
			return;
		}

		public override bool Equals(object obj)
		{
		    return RecursiveEquals(obj as Field, new ReferenceDictionary<Content, Content>())
                && CustomProperties == ((Field)obj).CustomProperties;
		}

		internal virtual bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return FieldId == other.FieldId;
		}

		public override int GetHashCode()
		{
			return FieldId.GetHashCode();
		}

		internal virtual int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
		{
			return GetHashCode();
		}
    }
}
