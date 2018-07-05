using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using QA.Configuration;

namespace QA.Core.Models.Configuration
{
    public class ProductDefinition//: AttachableConfigurableItem
    {
        /// <summary>
        /// идентификатор типа продукта
        /// </summary>
        public int ProdictTypeId { get; set; }
        /// <summary>
        /// Структура данных и поведение при клонировании, удалении
        /// </summary>
        public Content StorageSchema { get; set; }
    }

    //[NameScopeProperty("ContentName")]
    [ContentProperty("Fields")]
    [DictionaryKeyProperty("ContentId")]
    [TypeConverter(typeof(ContentTypeConverter))]
    public sealed class Content : AttachableConfigurableItem
    {
		[DisplayName("Грузить все простые поля")]
		[DefaultValue(true)]
        public bool LoadAllPlainFields { get; set; }
        
		[DisplayName("Имя контента")]
		[DefaultValue(null)]
		public string ContentName { get; set; }

		public int ContentId { get; set; }

		[DisplayName("Поведение при публикации")]
		[DefaultValue(PublishingMode.Publish)]
		public PublishingMode PublishingMode { get; set; }

		public List<Field> Fields { get; private set; }

        public Content()
        {
            Fields = new List<Field>();
            LoadAllPlainFields = true;
        }

        public override bool Equals(Object obj)
        {
            var objAsContent = obj as Content;

			if (objAsContent == null)
                return false;

	        return RecursiveEquals(objAsContent, new List<Tuple<Content, Content>>());
        }

        public override int GetHashCode()
        {
			return GetRecurciveHashCode(new List<Content> ());
        }

        public TimeSpan? GetCachePeriodForContent()
        {
            return XmlMappingBehavior.RetrieveCachePeriod(this);
        }

		public Content[] GetChildContentsIncludingSelf()
		{
			var childContents = new List<Content>();

			FillChildContents(childContents);

			return childContents.Distinct().ToArray();
		}

		internal void FillChildContents(List<Content> parents)
		{
			if (parents.Contains(this))
				return;

			parents.Add(this);

			foreach (Field field in Fields)
			{
				field.FillChildContents(parents);
			}
		}

		/// <summary>
		/// хеш код по всей глубине контена с учетом того что могут быть циклы
		/// </summary>
		/// <param name="visitedContents">родительские контенты</param>
		/// <returns></returns>
		internal int GetRecurciveHashCode(List<Content> visitedContents)
		{
			if (visitedContents.Any(x => ReferenceEquals(x, this)))
				return HashHelper.CombineHashCodes(ContentId.GetHashCode(), visitedContents.Count.GetHashCode());

			visitedContents.Add(this);

			int hash =  HashHelper.CombineHashCodes(PublishingMode.GetHashCode(), LoadAllPlainFields.GetHashCode());
			
			hash = HashHelper.CombineHashCodes(hash, ContentId.GetHashCode());

			return Fields.OrderBy(x => x.FieldId).Aggregate(hash, (current, field) => HashHelper.CombineHashCodes(current, field.GetRecurciveHashCode(visitedContents)));
		}

		/// <summary>
		/// глубокое сравнение контентов с учетом циклов
		/// </summary>
		/// <param name="other"></param>
		/// <param name="visitedContents">родительские контенты, Item1 текущего, Item2 - other</param>
		/// <returns></returns>
		internal bool RecursiveEquals(Content other, List<Tuple<Content,Content>> visitedContents)
		{
			if (ReferenceEquals(other, this))
				return true;

			foreach (var visitedContent in visitedContents)
			{
				if (ReferenceEquals(visitedContent.Item1, this))
					return ReferenceEquals(visitedContent.Item2, other);
			}

			visitedContents.Add(new Tuple<Content, Content>(this, other));

			return ContentId == other.ContentId
			       && LoadAllPlainFields == other.LoadAllPlainFields
			       && PublishingMode == other.PublishingMode
			       && Fields.Count == other.Fields.Count
			       && Fields.All(x => other.Fields.Any(y => y.FieldId == x.FieldId && x.RecursiveEquals(y, visitedContents)));
		}
	}
   
}
