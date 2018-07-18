using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.UI
{
    public class ItemDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
	    readonly Dictionary<TKey, TValue> _innerDictionary = new Dictionary<TKey, TValue>();
        protected UIElement Parent;

        public ItemDictionary(UIElement parentNode)
        {
            Parent = parentNode;
        }

        public void Add(TKey key, TValue value)
        {
			((IDictionary)this).Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _innerDictionary.Keys; }
        }

	    ICollection IDictionary.Values
	    {
		    get { throw new NotImplementedException(); }
	    }

	    public bool TryGetValue(TKey key, out TValue value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

	    ICollection IDictionary.Keys
	    {
		    get { throw new NotImplementedException(); }
	    }

	    public ICollection<TValue> Values
        {
            get { return _innerDictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return _innerDictionary[key];
            }
            set
            {
                _innerDictionary[key] = value;
            }
        }

	    void ICollection.CopyTo(Array array, int index)
	    {
		    throw new NotImplementedException();
	    }

	    public int Count
        {
            get { return _innerDictionary.Count; }
        }

	    object ICollection.SyncRoot
	    {
		    get { throw new NotImplementedException(); }
	    }

	    bool ICollection.IsSynchronized
	    {
		    get { throw new NotImplementedException(); }
	    }


	    public bool Remove(TKey key)
        {
            throw new NotSupportedException("This feature is not supported.");
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
			((IDictionary)this).Add(item.Key, item.Value);
        }

	    bool IDictionary.Contains(object key)
	    {
		    throw new NotImplementedException();
	    }

	    void IDictionary.Add(object key, object value)
	    {
			if (value is UIElement)
			{
				(value as UIElement).Parent = Parent;
			}

			((IDictionary)_innerDictionary).Add(key, value);
	    }

	    public void Clear()
        {
            throw new NotSupportedException("This feature is not supported.");
        }

	    IDictionaryEnumerator IDictionary.GetEnumerator()
	    {
		    throw new NotImplementedException();
	    }

	    void IDictionary.Remove(object key)
	    {
		    throw new NotImplementedException();
	    }

	    object IDictionary.this[object key]
	    {
		    get { throw new NotImplementedException(); }
		    set { throw new NotImplementedException(); }
	    }

	    public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _innerDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException("This feature is not supported.");
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

	    bool IDictionary.IsFixedSize
	    {
		    get { throw new NotImplementedException(); }
	    }

	    public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("This feature is not supported.");
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }
    }
}
