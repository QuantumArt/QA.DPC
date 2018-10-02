using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QA.Core.Models.Tools
{
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public bool Equals(T x, T y) => ReferenceEquals(x, y);
        
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

    /// <summary>
    /// <see cref="Dictionary{TKey, TValue}"/> that always use <see cref="RuntimeHelpers.GetHashCode"/>
    /// and <see cref="Object.ReferenceEquals(object, object)"/> for classes
    /// and <see cref="IStructuralEquatable"/> for <see cref="Tuple"/> and <see cref="ValueTuple"/>
    /// </summary>
    public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TKey : class
    {
        private static ReferenceEqualityComparer<TKey> EqualityComparer = new ReferenceEqualityComparer<TKey>();

        public ReferenceDictionary()
            : base(EqualityComparer)
        {
        }

        public ReferenceDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary, EqualityComparer)
        {
        }
    }

    /// <summary>
    /// <see cref="HashSet{T}"/> that always use <see cref="RuntimeHelpers.GetHashCode"/>
    /// and <see cref="Object.ReferenceEquals(object, object)"/> for classes
    /// and <see cref="IStructuralEquatable"/> for <see cref="Tuple"/> and <see cref="ValueTuple"/>
    /// </summary>
    public class ReferenceHashSet<T> : HashSet<T>
        where T : class
    {
        private static ReferenceEqualityComparer<T> EqualityComparer = new ReferenceEqualityComparer<T>();

        public ReferenceHashSet()
            : base(EqualityComparer)
        {
        }

        public ReferenceHashSet(IEnumerable<T> collection)
            : base(collection, EqualityComparer)
        {
        }
    }
}
