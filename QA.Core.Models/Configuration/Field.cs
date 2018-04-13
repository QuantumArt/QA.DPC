using QA.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Markup;

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

        [DisplayName("Имя поля")]
		public virtual string FieldName { get; set; }

		public virtual int FieldId { get; set; }

	    [DefaultValue(null)]
        public virtual string FieldType { get; set; }

	    [DefaultValue(null)]
        public NumberType? NumberType { get; set; }

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

	public sealed class PlainField : Field
	{
		[ScaffoldColumn(false)] 
		[DefaultValue(false)]
		public bool ShowInList { get; set; }

		internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

			return ShowInList == ((PlainField)other).ShowInList;
		}
	}

	public abstract class Association : Field
	{
		[DisplayName("При клонировании родительской сущности")]
		[DefaultValue(CloningMode.Ignore)]
		public CloningMode CloningMode { get; set; }


        [DisplayName("При создании\\обновлении")]
        [DefaultValue(UpdatingMode.Ignore)]
        public UpdatingMode UpdatingMode { get; set; }

        [DisplayName("При удалении родительской сущности")]
		[DefaultValue(DeletingMode.Keep)]
		public DeletingMode DeletingMode { get; set; }

        public abstract Content[] Contents { get; }

		protected Association()
		{
			CloningMode = CloningMode.Ignore;

			DeletingMode = DeletingMode.Keep;
		}

		internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

			var otherAsAssociation = (Association)other;

			return CloningMode == otherAsAssociation.CloningMode
				&& DeletingMode == otherAsAssociation.DeletingMode
                && UpdatingMode == otherAsAssociation.UpdatingMode;
		}

		public override int GetHashCode()
		{
			int hash = base.GetHashCode();

			hash = HashHelper.CombineHashCodes(hash, CloningMode.GetHashCode());
			hash = HashHelper.CombineHashCodes(hash, DeletingMode.GetHashCode());
            hash = HashHelper.CombineHashCodes(hash, UpdatingMode.GetHashCode());

            return hash;
		}
	}

	[ContentProperty("Content")]
	public class EntityField : Association
	{
        /// <summary>
        /// опциональный дефинишен для клонирования, если null то используется Content
        /// </summary>
        [DefaultValue(null)]
        public Content CloneDefinition { get; set; }

		public Content Content { get; set; }

		internal override void FillChildContents(List<Content> parents)
		{
            if (Content == null)
            {
                return;
            }

			Content.FillChildContents(parents);
		}

	    public override Content[] Contents => new[] { Content };

        internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
			return base.RecursiveEquals(other, visitedContents)
                && Content.RecursiveEquals(((EntityField)other).Content, visitedContents)
                && CloneDefinition == null
                    ? ((EntityField)other).CloneDefinition == null
                    : CloneDefinition.RecursiveEquals(((EntityField)other).CloneDefinition, visitedContents);
		}

		public override int GetHashCode()
		{
			return GetRecurciveHashCode(new ReferenceHashSet<Content>());
		}

		internal override int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
		{
			int hash = base.GetHashCode();

            if (Content != null)
            {
                hash = HashHelper.CombineHashCodes(hash, Content.GetRecurciveHashCode(visitedContents));
            }
            if (CloneDefinition != null)
            {
                hash = HashHelper.CombineHashCodes(hash, CloneDefinition.GetRecurciveHashCode(visitedContents));
            }

            return hash;
		}
	}

	[ContentProperty("ContentMapping")]
	public sealed class ExtensionField : Association
	{
		/// <summary>
		/// Маппинг 
		/// </summary>
		public Dictionary<int, Content> ContentMapping { get; private set; }

		public ExtensionField()
		{
			ContentMapping = new Dictionary<int, Content>();
		}

		internal override void FillChildContents(List<Content> parents)
		{
			foreach (Content content in ContentMapping.Values)
			{
				content.FillChildContents(parents);
			}
		}

	    public override Content[] Contents => ContentMapping.Values.ToArray();

        internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

			var otherExtensionField = (ExtensionField)other;

			return ContentMapping.Values.Count == otherExtensionField.ContentMapping.Values.Count
				&& ContentMapping.Values
                    .All(x => otherExtensionField.ContentMapping.Values
                        .Any(y => y.ContentId == x.ContentId && x.RecursiveEquals(y, visitedContents)));
		}
        
		public override int GetHashCode()
		{
			return GetRecurciveHashCode(new ReferenceHashSet<Content>());
		}

		internal override int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
		{
			int hash = base.GetHashCode();

            foreach (Content content in ContentMapping.Values.OrderBy(x => x.ContentId))
            {
                int contentHash = content.GetRecurciveHashCode(visitedContents);
                hash = HashHelper.CombineHashCodes(hash, contentHash);
            }

            return hash;
		}
	}

	[ContentProperty("ContentDictionaries")]
	public class Dictionaries : Field
	{
		private static readonly TimeSpan _minimum = TimeSpan.FromSeconds(10);
		private TimeSpan _defaultCachePeriod;

		[DefaultValue(null)]
		public override string FieldName { get { return null; } set {} }

		[DisplayName("Период кеширования по умолчанию")]
		public TimeSpan DefaultCachePeriod
		{
            get => _defaultCachePeriod < _minimum ? _minimum : _defaultCachePeriod;
            set { _defaultCachePeriod = value; }
		}

		/// <summary>
		/// Маппинг 
		/// </summary>
		public Dictionary<int, Content> ContentDictionaries { get; private set; }

		public Dictionaries()
		{
			ContentDictionaries = new Dictionary<int, Content>();
			DefaultCachePeriod = TimeSpan.FromMinutes(10);
		}

		public TimeSpan? GetCachePeriodForContent(int id)
		{
            if (ContentDictionaries.TryGetValue(id, out Content c))
            {
                // берем XmlMappingBehavior.CachePeriod="00:10:00" или DefaultCachePeriod="00:05:00"
                return XmlMappingBehavior.RetrieveCachePeriod(c) ?? DefaultCachePeriod;
            }
            else
            {
                return null;
            }
        }
        
        public override int FieldId => 0;

		internal override void FillChildContents(List<Content> parents)
		{
			return;
		}

		internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
		{
            if (!base.RecursiveEquals(other, visitedContents))
            {
                return false;
            }

			var otherField = (Dictionaries)other;

			return ContentDictionaries.Values.Count == otherField.ContentDictionaries.Values.Count
				&& ContentDictionaries.Values
                    .All(x => otherField.ContentDictionaries.Values
                        .Any(y => y.ContentId == x.ContentId && x.RecursiveEquals(y, visitedContents)));
		}

		public override int GetHashCode()
		{
			return GetRecurciveHashCode(new ReferenceHashSet<Content>());
		}

		internal override int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
		{
            int hash = FieldId.GetHashCode();

            foreach (Content content in ContentDictionaries.Values.OrderBy(x => x.ContentId))
            {
                int contentHash = content.GetRecurciveHashCode(visitedContents);
                hash = HashHelper.CombineHashCodes(hash, contentHash);
            }

            return hash;
		}
	}
}
