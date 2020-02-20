using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif
using QA.Core.Models.Tools;

namespace QA.Core.Models.Configuration
{
    [ContentProperty("ContentDictionaries")]
    public class Dictionaries : Field
    {
        private static readonly TimeSpan _minimum = TimeSpan.FromSeconds(10);
        private TimeSpan _defaultCachePeriod;

        [DefaultValue(null)]
        public override string FieldName { get { return null; } set {} }

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

        protected override void ShallowCopyMembers(Field field)
        {
            base.ShallowCopyMembers(field);
            var dictionaries = (Dictionaries)field;

            dictionaries.ContentDictionaries = ContentDictionaries?
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        protected override void DeepCopyMembers(Field field, ReferenceDictionary<object, object> visited)
        {
            base.DeepCopyMembers(field, visited);
            var dictionaries = (Dictionaries)field;

            dictionaries.ContentDictionaries = ContentDictionaries?
                .ToDictionary(pair => pair.Key, pair => pair.Value.DeepCopy(visited));
        }

        public TimeSpan? GetCachePeriodForContent(int id)
        {
            if (ContentDictionaries.TryGetValue(id, out Content c))
            {
                return c.CachePeriod ?? DefaultCachePeriod;
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