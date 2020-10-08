using System.Collections.Generic;
using System.ComponentModel;
using Portable.Xaml.Markup;
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
    [ContentProperty("Content")]
    public class EntityField : Association
    {
        [DefaultValue(PreloadingMode.None)]
        public PreloadingMode PreloadingMode { get; set; }

        /// <summary>
        /// SQL-условие для фильтрации списка статей, доступных для выбора
        /// (заменяет <see cref="Quantumart.QP8.BLL.Field.RelationCondition"/>)
        /// </summary>
        [DefaultValue(null)]
        public string RelationCondition { get; set; }

        /// <summary>
        /// Опциональный дефинишен для клонирования, если null то используется Content
        /// </summary>
        [DefaultValue(null)]
        public Content CloneDefinition { get; set; }
        
        /// <summary>
        /// SQL-условие для получения статьи-прототипа для клонирования
        /// </summary>
        [DefaultValue(null)]
        public string ClonePrototypeCondition { get; set; }
        
        public Content Content { get; set; }

        public EntityField()
        {
            PreloadingMode = PreloadingMode.None;
        }

        protected override void DeepCopyMembers(Field field, ReferenceDictionary<object, object> visited)
        {
            base.DeepCopyMembers(field, visited);
            var entityField = (EntityField)field;

            entityField.CloneDefinition = CloneDefinition?.DeepCopy(visited);
            entityField.Content = Content?.DeepCopy(visited);
        }

        internal override void FillChildContents(List<Content> parents)
        {
            if (Content == null)
            {
                return;
            }

            Content.FillChildContents(parents);
        }

        public override Content[] GetContents() => new[] { Content };

        internal override bool RecursiveEquals(Field other, ReferenceDictionary<Content, Content> visitedContents)
        {
            return base.RecursiveEquals(other, visitedContents)
                   && PreloadingMode == ((EntityField)other).PreloadingMode
                   && RelationCondition == ((EntityField)other).RelationCondition
                   && ClonePrototypeCondition == ((EntityField)other).ClonePrototypeCondition
                   && Content.RecursiveEquals(((EntityField)other).Content, visitedContents)
                   && (CloneDefinition == null
                       ? ((EntityField)other).CloneDefinition == null
                       : CloneDefinition.RecursiveEquals(((EntityField)other).CloneDefinition, visitedContents));
        }

        public override int GetHashCode()
        {
            return GetRecurciveHashCode(new ReferenceHashSet<Content>());
        }

        internal override int GetRecurciveHashCode(ReferenceHashSet<Content> visitedContents)
        {
            int hash = base.GetHashCode();

            hash = HashHelper.CombineHashCodes(hash, PreloadingMode.GetHashCode());

            if (RelationCondition != null)
            {
                hash = HashHelper.CombineHashCodes(hash, RelationCondition.GetHashCode());
            }
            if (ClonePrototypeCondition != null)
            {
                hash = HashHelper.CombineHashCodes(hash, ClonePrototypeCondition.GetHashCode());
            }
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
}