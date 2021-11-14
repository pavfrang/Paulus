using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Paulus.IO
{
    public static class TextFileUtilities
    {
        private static CultureInfo en = CultureInfo.InvariantCulture;

        public static IEnumerable<int> ReadColumnLocations(string textFilePath, int headerRow, IEnumerable<string> headers)
        {
            using (StreamReader reader = new StreamReader(textFilePath, Encoding.Default))
            {
                //omit first lines
                for (int iLine = 1; iLine < headerRow; iLine++)
                {
                    reader.ReadLine();
                    if (reader.EndOfStream) break;
                }

                string[] tokens = reader.ReadLine().Trim().Split('\t');

                foreach (string header in headers)
                    yield return Array.IndexOf<string>(tokens, header) + 1;
            }
        }

        public static List<List<double>> ReadData(string textFilePath, int firstValuesRow, int headerRow, params string[] headers)
        {
            int[] columns = ReadColumnLocations(textFilePath, headerRow, headers).ToArray();
            return ReadData(textFilePath, firstValuesRow, columns);
        }

        //columns and firstValuesRow are 1-based
        public static List<List<double>> ReadData(string textFilePath, int firstValuesRow, params int[] columns)
        {
            //if(!File.Exists(textFilePath)) throw new FileNotFoundException("File does not exist.",textFilePath);


            List<List<double>> lists = new List<List<double>>();
            bool readAllColumns = columns == null || columns.Length == 0;


            using (StreamReader reader = new StreamReader(textFilePath))
            {
                //omit first lines
                for (int iLine = 1; iLine < firstValuesRow; iLine++)
                {
                    reader.ReadLine();
                    if (reader.EndOfStream) break;
                }


                //read all columns
                if (readAllColumns)
                {
                    //initialize lists by reading the first values line
                    string line = reader.ReadLine().Trim();
                    string[] tokens = line.Trim().Split('\t');
                    int iListCount = tokens.Length;
                    for (int iList = 0; iList < iListCount; iList++)
                    {
                        lists.Add(new List<double>());
                        lists[iList].Add(double.Parse(tokens[iList], en));
                    }

                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine().Trim();
                        if (line.Length == 0) continue;
                        tokens = line.Split('\t');

                        for (int iList = 0; iList < iListCount; iList++)
                            lists[iList].Add(double.Parse(tokens[iList], en));
                    }
                }
                else //read only selected columns
                {
                    //initialize lists
                    int iListCount = columns.Length;
                    for (int iList = 0; iList < iListCount; iList++)
                        lists.Add(new List<double>());

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        if (line.Length == 0) continue;
                        string[] tokens = line.Split('\t');

                        for (int iList = 0; iList < iListCount; iList++)
                            lists[iList].Add(double.Parse(tokens[columns[iList] - 1], en));
                    }
                }
            }

            return lists;
        }
    }
}
