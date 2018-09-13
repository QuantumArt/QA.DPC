using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using QA.Configuration;
using QA.Core.Models.Tools;

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

        public Content ShallowCopy()
        {
            var content = (Content)MemberwiseClone();

            content.Fields = Fields?.ToList();

            return content;
        }

        public Content DeepCopy()
        {
            return DeepCopy(new ReferenceDictionary<object, object>());
        }

        internal Content DeepCopy(ReferenceDictionary<object, object> visited)
        {
            if (visited.TryGetValue(this, out object value))
            {
                return (Content)value;
            }

            var content = (Content)MemberwiseClone();

            visited[this] = content;

            content.Fields = Fields?.ConvertAll(field => field.DeepCopy(visited));
            
            return content;
        }

        public override bool Equals(object obj)
        {
            return obj is Content content
                && RecursiveEquals(content, new ReferenceDictionary<Content, Content>());
        }

        public override int GetHashCode()
        {
            return GetRecurciveHashCode(new ReferenceHashSet<Content>());
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
            {
                return;
            }

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
        internal int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
        {
            if (visitedContents.Contains(this))
            {
                return HashHelper.CombineHashCodes(ContentId.GetHashCode(), visitedContents.Count.GetHashCode());
            }

            visitedContents.Add(this);

            int hash = PublishingMode.GetHashCode();
            hash = HashHelper.CombineHashCodes(hash, LoadAllPlainFields.GetHashCode());
            hash = HashHelper.CombineHashCodes(hash, ContentId.GetHashCode());

            foreach (Field field in Fields.OrderBy(x => x.FieldId))
            {
                int fieldHash = field.GetRecurciveHashCode(visitedContents);
                hash = HashHelper.CombineHashCodes(hash, fieldHash);
            }

            return hash;
        }

        /// <summary>
        /// глубокое сравнение контентов с учетом циклов
        /// </summary>
        /// <param name="other"></param>
        /// <param name="visitedContents">родительские контенты, Key текущего, Value - other</param>
        /// <returns></returns>
        internal bool RecursiveEquals(Content other, ReferenceDictionary<Content, Content> visitedContents)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (visitedContents.ContainsKey(this))
            {
                return ReferenceEquals(visitedContents[this], other);
            }

            visitedContents.Add(this, other);

            return ContentId == other.ContentId
                && LoadAllPlainFields == other.LoadAllPlainFields
                && PublishingMode == other.PublishingMode
                && Fields.Count == other.Fields.Count
                && Fields.All(x => other.Fields
                    .Any(y => y.FieldId == x.FieldId && x.RecursiveEquals(y, visitedContents)));
        }
    }
}
