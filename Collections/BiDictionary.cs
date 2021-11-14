using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Paulus.Collections
{
    /// <summary>
    /// Simple implementation of bidirectional dictionary. Useful only if T1 is other than T2.
    /// </summary>
    public class BiDictionary<T1, T2> : Dictionary<T1, T2>
    {
        public BiDictionary() { }

        public BiDictionary(int capacity) : base(capacity) { }

        public BiDictionary(IEqualityComparer<T1> comparer) : base(comparer) { }

        public BiDictionary(IDictionary<T1, T2> dictionary) : base(dictionary) { }

        public BiDictionary(int capacity, IEqualityComparer<T1> comparer) : base(capacity, comparer) { }

        public BiDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public BiDictionary(IDictionary<T1, T2> dictionary, IEqualityComparer<T1> comparer) : base(dictionary, comparer) { }



        public T1 this[T2 index]
        {
            get
            {
                if (!this.Any(x => x.Value.Equals(index)))
                    throw new KeyNotFoundException();
                return this.First(x => x.Value.Equals(index)).Key;
            }
        }
    }

    //public class BiDictionary<TFirst, TSecond>
    //{
    //    IDictionary<TFirst, IList<TSecond>> firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
    //    IDictionary<TSecond, IList<TFirst>> secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

    //    private static IList<TFirst> EmptyFirstList = new TFirst[0];
    //    private static IList<TSecond> EmptySecondList = new TSecond[0];

    //    public void Add(TFirst first, TSecond second)
    //    {
    //        IList<TFirst> firsts;
    //        IList<TSecond> seconds;
    //        if (!firstToSecond.TryGetValue(first, out seconds))
    //        {
    //            seconds = new List<TSecond>();
    //            firstToSecond[first] = seconds;
    //        }
    //        if (!secondToFirst.TryGetValue(second, out firsts))
    //        {
    //            firsts = new List<TFirst>();
    //            secondToFirst[second] = firsts;
    //        }
    //        seconds.Add(second);
    //        firsts.Add(first);
    //    }

    //    // Note potential ambiguity using indexers (e.g. mapping from int to int)
    //    // Hence the methods as well...
    //    public IList<TSecond> this[TFirst first]
    //    {
    //        get { return GetByFirst(first); }
    //    }

    //    public IList<TFirst> this[TSecond second]
    //    {
    //        get { return GetBySecond(second); }
    //    }

    //    public IList<TSecond> GetByFirst(TFirst first)
    //    {
    //        IList<TSecond> list;
    //        if (!firstToSecond.TryGetValue(first, out list))
    //        {
    //            return EmptySecondList;
    //        }
    //        return new List<TSecond>(list); // Create a copy for sanity
    //    }

    //    public IList<TFirst> GetBySecond(TSecond second)
    //    {
    //        IList<TFirst> list;
    //        if (!secondToFirst.TryGetValue(second, out list))
    //        {
    //            return EmptyFirstList;
    //        }
    //        return new List<TFirst>(list); // Create a copy for sanity
    //    }
    //}
}
