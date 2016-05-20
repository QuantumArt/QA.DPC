using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.UI
{
    public class ItemList<T> : ICollection<T>, IList<T>, IList where T : UIElement
    {
        private UIElement _item;
        private IList<T> _innerList;

        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _innerList.Insert(index, item);
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return _item.OnGetItem(_innerList[index]);
            }
            set { _innerList[index] = (T)value; }
        }

        public T this[int index]
        {
            get
            {
                return _item.OnGetItem(_innerList[index]);
            }
            set
            {
                _innerList[index] = value;
            }
        }

        public ItemList(UIElement item)
        {
            _item = item;
            _innerList = new List<T>();
        }
        public ItemList(UIElement item, IEnumerable<T> items)
        {
            _item = item;
            _innerList = new List<T>(items);
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            ((IList)this).Add(item);
        }

        int IList.Add(object value)
        {
            ((T)value).Parent = _item;

            return ((IList)_innerList).Add(value);
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            foreach (var item in _innerList)
            {
                item.Parent = null;
            }

            _innerList.Clear();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)_innerList).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_innerList).IsSynchronized; }
        }

        public bool IsReadOnly
        {
            get { return _innerList.IsReadOnly; }
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)_innerList).IsFixedSize; }
        }

        public bool Remove(T item)
        {
            item.Parent = null;
            return _innerList.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.Select(x => _item.OnGetItem(x)).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerList.Select(x => _item.OnGetItem(x))).GetEnumerator();
        }

        #endregion
    }
}
