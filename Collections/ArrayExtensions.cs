
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;


namespace Paulus.Collections
{
    public static class ArrayExtensions
    {
        public static void CopyTo<T>(this T[] source, T[] target, int sourceStart, int sourceEnd, int targetStart)
        {
            int count = sourceEnd - sourceStart + 1;
            if (count > 0)
                for (int i = sourceStart, j = targetStart; i <= sourceEnd; i++, j++)
                    target[j] = source[i];
        }

        /// <summary>
        /// Returns a part of the source array.
        /// </summary>
        /// <typeparam name="T">The type of each element of the array</typeparam>
        /// <param name="source">The array whose part is to be extracted</param>
        /// <param name="start">The start index to begin the copy operation.</param>
        /// <param name="end">The end index to finish the copy operation. The final element is included to the partial array.</param>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            int count = end - start + 1;
            T[] target = null;

            if (count > 0)
            {
                target = new T[count];
                for (int i = start; i <= end; i++)
                    target[i - start] = source[i];
            }
            return target;
        }


        public static T[] Slice<T>(this T[] source, int index, bool indexIsStart = true)
        {
            return indexIsStart ? source.Slice<T>(index, source.Length - 1) : source.Slice<T>(0, index);
        }

        private static CultureInfo enConvert = CultureInfo.InvariantCulture; //used for parsing

        //crops data to a specified range (to avoid invalid values)
        public static double[] LoadFromFile(string path, int count, double min = double.MinValue, double max = double.MaxValue)
        {
            double[] values = new double[count];
            using (StreamReader reader = new StreamReader(path, Encoding.Default))
            {
                for (int i = 0; i < count; i++)
                {
                    double value = double.Parse(reader.ReadLine(), enConvert);
                    value = Math.Min(max, Math.Max(min, value));
                    values[i] = value;
                }
            }
            return values;
        }

        public static double[] LoadFromFile(string path, double min = double.MinValue, double max = double.MaxValue) //safest implementation
        {
            List<double> values = new List<double>();
            using (StreamReader reader = new StreamReader(path, Encoding.Default))
            {
                while (!reader.EndOfStream)
                {
                    double value = double.Parse(reader.ReadLine(), enConvert);
                    value = Math.Min(max, Math.Max(min, value));
                    values.Add(value);
                }
            }
            return values.ToArray();
        }

        public static void SaveToFile(this double[] array, string path, CultureInfo culture)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.Default))
                foreach (double v in array)
                    writer.WriteLine(v.ToString(culture));
        }

        public static void SaveToFile(this double[] array, string path) { array.SaveToFile(path, CultureInfo.InvariantCulture); }

        public static T[] GetConstantArray<T>(int count, T value)
        {
            T[] array = new T[count];
            for (int i = 0; i < count; i++) array[i] = value;
            return array;
        }



    }
}
