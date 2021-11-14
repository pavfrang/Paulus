using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Collections
{
    public static class ListExtensions
    {
        public static void AddIfUniqueAndNotNull<T>(this List<T> array, T value)
        {
            if (value != null && !array.Contains(value)) array.Add(value);
        }

        public static List<T> ToUniqueValuesList<T>(this List<T> array)
        {
            List<T> uniqueValues = new List<T>();

            foreach (T value in array)
                uniqueValues.AddIfUniqueAndNotNull<T>(value);

            return uniqueValues;
        }

        public static void Randomize<T>(this List<T> array, int swaps)
        {
            Random rnd = new Random();
            int count = array.Count;
            if (array.Count <= 1) return;//ignore the call for empty arrays

            for (int i = 0; i < swaps; i++)
            {
                int first = rnd.Next(count);
                //retrieve a second index until 
                int second = rnd.Next(count);
                while (second == first)
                    second = rnd.Next(count);

                Swap<T>(first, second, array);
            }

        }

        private static void Swap<T>(int i, int j, List<T> array)
        {
            if (i != j)
            {
                T tmp = array[i];
                array[i] = array[j];
                array[j] = tmp;
            }
        }

        public static bool ContainsPartial(this List<string> array, string item, bool caseSensitive = false)
        {
            StringComparison comparison = caseSensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
            foreach (string s in array)
                if (s.IndexOf(item, comparison) >= 0) return true;

            return false;
        }


        public static void AddRangeLists<T>(this List<T> list, IEnumerable<IEnumerable<T>> listsToCopyFrom)
        {
            //16-11-2013: Created.
            foreach (var l in listsToCopyFrom)
                list.AddRange(l);
        }


        //02-03-2014: Created.
        public static bool IsSortedInIncreasingOrder<T>(this IEnumerable<T> list, bool strictlyIncreasing = true) where T : IComparable
        {
            int c = list.Count();

            if (strictlyIncreasing)
                for (int i = 1; i < c; i++)
                {
                    int cmp = list.ElementAt(i).CompareTo(list.ElementAt(i - 1));

                    if (cmp <= 0)
                        return false;
                }
            else
                for (int i = 1; i < c; i++)
                {
                    int cmp = list.ElementAt(i).CompareTo(list.ElementAt(i - 1));

                    if (cmp < 0)
                        return false;
                }

            return true;
        }

        //02-03-2014: Created.
        public static bool IsSortedInDecreasingOrder<T>(this IEnumerable<T> list, bool strictlyDecreasing = true) where T : IComparable
        {
            int c = list.Count();

            for (int i = 1; i < c; i++)
            {
                int cmp = list.ElementAt(i).CompareTo(list.ElementAt(i - 1));

                if (strictlyDecreasing ? cmp >=0 : cmp > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the indices of the array elements which are not in increasing order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="strictlyDecreasing">If true then equal consecutive values are considered invalid.</param>
        /// <returns></returns>
        public static List<int> GetIndicesOfNonIncreasingOrder<T>(this IEnumerable<T> list, bool strictlyIncreasing = true) where T : IComparable
        {
            List<int> indices = new List<int>();

            int c = list.Count();

            for (int i = 1; i < c; i++)
            {
                int cmp = list.ElementAt(i).CompareTo(list.ElementAt(i - 1));

                if (strictlyIncreasing ? cmp <= 0 : cmp < 0)
                    indices.Add(i);
            }

            return indices;
        }

        /// <summary>
        /// Returns the indices of the array elements which are not in decreasing order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <param name="strictlyIncreasing">If true then equal consecutive values are considered invalid.</param>
        /// <returns></returns>
        public static List<int> GetIndicesOfNonDecreasingOrder<T>(this IEnumerable<T> list, bool strictlyDecreasing = true) where T : IComparable
        {
            List<int> indices = new List<int>();

            int c = list.Count();

            for (int i = 1; i < c; i++)
            {
                int cmp = list.ElementAt(i).CompareTo(list.ElementAt(i - 1));

                if (strictlyDecreasing ? cmp >=0 : cmp > 0)
                    indices.Add(i);
            }

            return indices;
        }

    }
}
