using System.Collections.Generic;
using System.Linq;
#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
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

        protected override void ShallowCopyMembers(Field field)
        {
            base.ShallowCopyMembers(field);
            var extensionField = (ExtensionField)field;

            extensionField.ContentMapping = ContentMapping?
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        protected override void DeepCopyMembers(Field field, ReferenceDictionary<object, object> visited)
        {
            base.DeepCopyMembers(field, visited);
            var extensionField = (ExtensionField)field;

            extensionField.ContentMapping = ContentMapping?
                .ToDictionary(pair => pair.Key, pair => pair.Value.DeepCopy(visited));
        }

        internal override void FillChildContents(List<Content> parents)
        {
            foreach (Content content in ContentMapping.Values)
            {
                content.FillChildContents(parents);
            }
        }

        public override Content[] GetContents() => ContentMapping.Values.ToArray();

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
}