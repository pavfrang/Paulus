using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Paulus.Common;

namespace Paulus.Collections
{
    [Serializable]
    public class ListWithEvents<TValue> : IListWithEvents<TValue>
    {
        private const int DefaultInitialCapacity = 0;

        private static readonly string _valueTypeName = typeof(TValue).FullName;
        private static readonly bool _valueTypeIsReferenceType = !typeof(ValueType).IsAssignableFrom(typeof(TValue));

        private List<TValue> _list;
        private readonly IEqualityComparer<TValue> _comparer;
        private readonly int _initialCapacity;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> class.
        /// </summary>
        public ListWithEvents()
            : this(DefaultInitialCapacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> class using the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> can contain.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
        public ListWithEvents(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> class using the specified comparer.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
        public ListWithEvents(IEqualityComparer<TValue> comparer)
            : this(DefaultInitialCapacity, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> class using the specified initial capacity and comparer.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
        public ListWithEvents(int capacity, IEqualityComparer<TValue> comparer)
        {
            if (0 > capacity)
                throw new ArgumentOutOfRangeException("capacity", @"'capacity' must be non-negative");

            _initialCapacity = capacity;
            _comparer = comparer;
        }
        #endregion

        /// <summary>
        /// Converts the object passed as a value to the value type of the dictionary
        /// </summary>
        /// <param name="value">The object to convert to the value type of the dictionary</param>
        /// <returns>The value object, converted to the value type of the dictionary</returns>
        /// <exception cref="ArgumentNullException"><paramref name="valueObject"/> is <null/>, and the value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is a value type.</exception>
        /// <exception cref="ArgumentException">The value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
        private static TValue ConvertToValueType(object value)
        {
            if (null == value)
            {
                if (_valueTypeIsReferenceType)
                    return default(TValue);
                throw new ArgumentNullException("value");
            }
            if (value is TValue)
                return (TValue)value;
            throw new ArgumentException(@"'value' must be of type " + _valueTypeName, "value");
        }


        /// <summary>
        /// Gets the list object that stores the key/value pairs.
        /// </summary>
        /// <value>The list object that stores the key/value pairs for the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see></value>
        /// <remarks>Accessing this property will create the list object if necessary.</remarks>
        private List<TValue> List
        {
            get { return _list ?? (_list = new List<TValue>(_initialCapacity)); }
        }

        public static implicit operator List<TValue>(ListWithEvents<TValue> d)
        {
            return d._list;
        }

        #region Enumerators

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        //by Paulus (GetEnumerator should be public)
        public IEnumerator<TValue> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Inserts a new entry into the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see>.</exception>
        public void Insert(int index, TValue value)
        {
            if (index > Count || index < 0)
                throw new ArgumentOutOfRangeException("index");

            List.Insert(index, value);
            if (!IgnoreEventHandlers) OnItemAdded(value);
        }

        ///// <summary>
        ///// Inserts a new entry into the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection with the specified key and value at the specified index.
        ///// </summary>
        ///// <param name="index">The zero-based index at which the element should be inserted.</param>
        ///// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        ///// -or-<br/>
        ///// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        ///// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.<br/>
        ///// -or-<br/>
        ///// <paramref name="value"/> is <null/>, and the value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is a value type.</exception>
        ///// <exception cref="ArgumentException">The key type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="key"/>.<br/>
        ///// -or-<br/>
        ///// The value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="value"/>.<br/>
        ///// -or-<br/>
        ///// An element with the same key already exists in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see>.</exception>
        //void IListWithEvents.Insert(int index, object key, object value)
        //{
        //    Insert(index, ConvertToKeyType(key), ConvertToValueType(value));
        //}

        /// <summary>
        /// Removes the entry at the specified index from the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index >= Count || index < 0)
                throw new ArgumentOutOfRangeException("index", @"'index' must be non-negative and less than the size of the collection");

            var entry = List[index]; //by paulus
            List.RemoveAt(index);
            if (!IgnoreEventHandlers) OnItemRemoved(entry, index); //by paulus
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the item at the specified index.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        public TValue this[int index]
        {
            get
            {
                return List[index];
            }

            set
            {
                if (index >= Count || index < 0)
                    throw new ArgumentOutOfRangeException("index", @"'index' must be non-negative and less than the size of the collection");

                List[index] = value;
            }
        }

        ///// <summary>
        ///// Gets or sets the value at the specified index.
        ///// </summary>
        ///// <param name="index">The zero-based index of the value to get or set.</param>
        ///// <value>The value of the item at the specified index.</value>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        ///// -or-<br/>
        ///// index is equal to or greater than <see cref="Count"/>.</exception>
        ///// <exception cref="ArgumentNullException"><paramref name="valueObject"/> is a null reference, and the value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is a value type.</exception>
        ///// <exception cref="ArgumentException">The value type of the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
        //object IListWithEvents.this[int index]
        //{
        //    get
        //    {
        //        return this[index];
        //    }

        //    set
        //    {
        //        this[index] = ConvertToValueType(value);
        //    }
        //}

        //------------events by paulus!
        public event EventHandler<EventArgs<TValue>> ItemAdded;
        protected void OnItemAdded(TValue entry)
        {
            var handler = ItemAdded;
            if (handler != null) handler(this, new EventArgs<TValue>(entry));
        }

        public event EventHandler<EventArgs<TValue>> ItemRemoved;
        protected void OnItemRemoved(TValue entry, int position)
        {
            var handler = ItemRemoved;
            if (handler != null) handler(this, new EventArgs<TValue>(entry));
        }

        public bool IgnoreEventHandlers; //to speed up when loading from setting files
        //-----------

        /// <summary>
        /// Adds an entry with the specified key and value into the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection with the lowest available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null/>.</param>
        /// <returns>The index of the newly added entry</returns>
        /// <remarks>A key cannot be <null/>, but a value can be.
        /// <para>You can also use the <see cref="P:ListWithEvents{TValue}.Item(TKey)"/> property to add new elements by setting the value of a key that does not exist in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection; however, if the specified key already exists in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see>, setting the <see cref="P:ListWithEvents{TValue}.Item(TKey)"/> property overwrites the old value. In contrast, the <see cref="M:Add"/> method does not modify existing elements.</para></remarks>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see></exception>
        public int Add(TValue value)
        {
            List.Add(value);
            if (!IgnoreEventHandlers) OnItemAdded(value); //by paulus
            return Count - 1;
        }

        /// <summary>
        /// Removes all elements from the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection.
        /// </summary>
        /// <remarks>The capacity is not changed as a result of calling this method.</remarks>
        public void Clear()
        {
            List.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection is read-only.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> is read-only; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>
        /// A collection that is read-only does not allow the addition, removal, or modification of elements after the collection is created.
        /// <para>A collection that is read-only is simply a collection with a wrapper that prevents modification of the collection; therefore, if changes are made to the underlying collection, the read-only collection reflects those changes.</para>
        /// </remarks>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the zero-based index of the specified value in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see>
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see></param>
        /// <returns>The zero-based index of <paramref name="value"/>, if <paramref name="ley"/> is found in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see>; otherwise, -1</returns>
        /// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
        public int IndexOf(TValue value)
        {
            if (null == value)
                throw new ArgumentNullException("value");

            for (int index = 0; index < List.Count; index++)
            {
                var entry = List[index];
                TValue next = entry;
                if (null != _comparer)
                {
                    if (_comparer.Equals(next, value))
                    {
                        return index;
                    }
                }
                if (next.Equals(value))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the entry with the specified key from the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove</param>
        /// <returns><see langword="true"/> if the key was found and the corresponding element was removed; otherwise, <see langword="false"/></returns>
        public bool Remove(TValue value)
        {
            if (null == value)
                throw new ArgumentNullException("value");
            int index = IndexOf(value);
            if (index >= 0)
            {
                var entry = List[index]; //by paulus
                List.RemoveAt(index);
                if (!IgnoreEventHandlers) OnItemRemoved(entry, index); //by paulus
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the number of key/values pairs contained in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection.
        /// </summary>
        /// <value>The number of key/value pairs contained in the <see cref="ListWithEvents{TValue}">ListWithEvents&lt;TValue&gt;</see> collection.</value>
        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        void ICollection<TValue>.Add(TValue item)
        {
            Add(item);
        }

        public bool Contains(TValue item)
        {
            return List.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }
    }

}
