using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Paulus.Collections
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Copies all the items of the source to the target.
        /// </summary>
        /// <typeparam name="T">The type of the index of source and target dictionaries.</typeparam>
        /// <typeparam name="V">The type of the items contained in the source and target dictionaries.</typeparam>
        /// <param name="source">The source Dictionary object.</param>
        /// <param name="target">The target Dictionary object.</param>
        public static void CopyTo<T, V>(this Dictionary<T, V> source, Dictionary<T, V> target)
        {
            if (target == null) target = new Dictionary<T, V>();
            foreach (KeyValuePair<T, V> entry in source)
                if (!target.ContainsKey(entry.Key)) //should generate a warning here
                    target.Add(entry.Key, entry.Value);
        }


        /// <summary>
        /// Copies the items of the source to the target, starting at a specified index.
        /// </summary>
        /// <typeparam name="T">The type of the index of source and target dictionaries.</typeparam>
        /// <typeparam name="V">The type of the items contained in the source and target dictionaries.</typeparam>
        /// <param name="source">The source Dictionary object.</param>
        /// <param name="target">The target Dictionary object.</param>
        /// <param name="start">The start index to begin the copy operation at.</param>
        public static void CopyTo<T, V>(this Dictionary<T, V> source, Dictionary<T, V> target, int start)
        {
            if (target == null) target = new Dictionary<T, V>();
            int iEntry = 0;
            foreach (KeyValuePair<T, V> entry in source)
            {
                if (iEntry++ >= start)
                {
                    if (!target.ContainsKey(entry.Key)) //adds only the unique keys
                        target.Add(entry.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Copies the items of the source to the target, starting at a specified index.
        /// </summary>
        /// <typeparam name="T">The type of the index of source and target dictionaries.</typeparam>
        /// <typeparam name="V">The type of the items contained in the source and target dictionaries.</typeparam>
        /// <param name="source">The source Dictionary object.</param>
        /// <param name="target">The target Dictionary object.</param>
        /// <param name="start">The start index to begin the copy operation at.</param>
        /// <param name="end">The last index to finish the copy operation at (included).</param>
        public static void CopyTo<T, V>(this Dictionary<T, V> source, Dictionary<T, V> target, int start, int end)
        {
            if (target == null) target = new Dictionary<T, V>();
            int iEntry = 0;
            foreach (KeyValuePair<T, V> entry in source)
            {
                if (iEntry >= start && iEntry <= end) target.Add(entry.Key, entry.Value);
                iEntry++;
            }
        }

        /// <summary>
        /// Sorts a dictionary according to the specified comparison-criterion.
        /// </summary>
        /// <typeparam name="T">The type of the index of source and target dictionaries.</typeparam>
        /// <typeparam name="V">The type of the items contained in the source and target dictionaries.</typeparam>
        /// <param name="source">The source Dictionary object.</param>
        /// <param name="comparison">The comparison function used to compare the entries.</param>
        /// <example> dic.Sort((p1, p2) =&gt; { return p1.Value - p2.Value; }); //lambda expression C#3.0
        /// //OR
        ///_compatibilities.Sort&lt;string, int&gt;(delegate(KeyValuePair&lt;string, int&gt; p1, KeyValuePair&lt;string, int&gt; p2) { return p1.Value - p2.Value;}); //anonymous method (C# 2.0)
        /// //OR
        /// dic.Sort(CompareItems);
        /// //OR (original oldest style)
        /// dic.Sort&lt;string, int&gt;(new Comparison&lt;KeyValuePair&lt;string, int&gt;&gt;(CompareItems));
        /// //...
        /// public int CompareItems(KeyValuePair&lt;string, int&gt; pair1, KeyValuePair&lt;string, int&gt; pair2)
        /// {
        ///    return pair1.Value.CompareTo(pair2.Value); //pair1.Value-pair2.Value
        /// }
        /// </example>
        public static void Sort<T, V>(this Dictionary<T, V> source, Comparison<KeyValuePair<T, V>> comparison)
        {
            //loads the entries to a list
            List<KeyValuePair<T, V>> entries = new List<KeyValuePair<T, V>>(source);
            //sort them according to the comparison function
            entries.Sort(comparison);
            //clear all unsorted entries
            source.Clear();
            //read the sorted entries
            foreach (var entry in entries) source.Add(entry.Key, entry.Value);
        }

        /// <summary>
        /// Sorts a dictionary according to the specified comparison object.
        /// </summary>
        /// <typeparam name="T">The type of the index of source and target dictionaries.</typeparam>
        /// <typeparam name="V">The type of the items contained in the source and target dictionaries.</typeparam>
        /// <param name="source">The source Dictionary object.</param>
        /// <param name="comparer">The comparison object used to compare the entries.</param>
        /// <example> dic.Sort(new MyComparer);
        /// ...
        /// public class MyComparer : IComparer&lt;KeyValuePair&lt;string,int&gt;&gt;
        /// {
        ///     public int Compare(KeyValuePair&lt;string, int&gt; x, KeyValuePair&lt;string, int&gt; y)
        ///     {
        ///         return x.Value - y.Value;
        ///     }
        /// }
        /// </example>
        public static void Sort<T, V>(this Dictionary<T, V> source, IComparer<KeyValuePair<T, V>> comparer)
        {
            //loads the entries to a list
            List<KeyValuePair<T, V>> entries = new List<KeyValuePair<T, V>>(source);
            //sort them according to the comparison function
            entries.Sort(comparer);
            //clear all unsorted entries
            source.Clear();
            //read the sorted entries
            foreach (var entry in entries) source.Add(entry.Key, entry.Value);
        }


        public static List<TKey> GetKeysFromValue<TKey, TVal>(this Dictionary<TKey, TVal> dict, TVal val)
        {
            List<TKey> ks = new List<TKey>();
            foreach (TKey k in dict.Keys)
            {
                TVal value;
                if (dict.TryGetValue(k, out value) && value.Equals(val))
                    ks.Add(k);
            }
            return ks;
        }

        public static TKey GetFirstKeyFromValue<TKey, TVal>(this Dictionary<TKey, TVal> dict, TVal val)
        {
            foreach (TKey k in dict.Keys)
            {
                TVal value;
                if (dict.TryGetValue(k, out value) && value.Equals(val))
                    return k;
            }
            return default(TKey);
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this Hashtable table)
        {
            return table
              .Cast<DictionaryEntry>()
              .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }
    }
}
