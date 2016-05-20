using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.UI
{
    public class MutableCollection<T, V> : IEnumerable<T> where T : UIElement
    {
        IEnumerable<T> _inner;
        public MutableCollection(IEnumerable<V> inner, T template)
        {
            _inner = inner.Select(x =>
            {
                template.DataContext = x;
                return template;
            });

        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_inner).GetEnumerator();
        }

        #endregion
    }
}
