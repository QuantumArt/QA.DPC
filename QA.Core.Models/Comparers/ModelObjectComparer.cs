using QA.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.Comparers
{
    public class ModelObjectComparer : IComparer<object>, IEqualityComparer<object>
    {
        public readonly static ModelObjectComparer Default = new ModelObjectComparer();

        #region IComparer<IModelObject> Members

        int IComparer<object>.Compare(object x, object y)
        {
            if (x is IComparable && y is IComparable && x.GetType().IsAssignableFrom(y.GetType()))
            {
                return ((IComparable)x).CompareTo(y);
            }

            if (x is Article && y is Article)
            {
                return Comparer<int>.Default.Compare(((Article)x).Id, ((Article)y).Id);
            }

            if (x is IEnumerable<Article>)
            {
                var ax = ((IEnumerable<Article>)x).FirstOrDefault();
                var ay = ((IEnumerable<Article>)y).FirstOrDefault();
                if (ax != null && ay != null)
                {
                    return Comparer<int>.Default.Compare(ax.Id, ay.Id);
                }
            }
            return 0;
        }

        #endregion

        #region IEqualityComparer<object> Members

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;

            if (x.GetType() != y.GetType())
                return false;

            if (x is Article || x is IEnumerable<Article>)
            {
                return ((IEqualityComparer<object>)this).GetHashCode(x) == ((IEqualityComparer<object>)this).GetHashCode(y);
            }

            return x.Equals(y);
        }


        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            if (obj is Article)
                return ((Article)obj).Id;

            if (obj is IEnumerable<Article>)
            {
                var ax = ((IEnumerable<Article>)obj).OrderBy(x => x.Id).Select(m => m.Id).Distinct().ToList();
                if (ax.Count == 0)
                    return 0;

                var hash = ax[0] + (ax[0] ^ ax.Aggregate((x, y) => (x + 1) ^ (y * 3 + 7)) + 1);

                return hash;
            }

            return obj.GetHashCode();
        }

        #endregion

    }
}
