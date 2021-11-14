using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Paulus.Excel
{
    public static class NamesExtensions
    {

        #region Names
        /// <summary>
        /// Returns true if the name exists.
        /// </summary>
        /// <param name="names">The names collection.</param>
        /// <param name="name">The name to be checked for existence.</param>
        /// <returns>true if the name exists.</returns>
        public static bool Contains(this Names names, string name)
        {
            StringComparison comparison = StringComparison.OrdinalIgnoreCase;

            foreach (Name nm in names)
            {
                bool found = nm.Name.Equals(name, comparison);
                if (found)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the name exists. Also returns the name object if found.
        /// </summary>
        /// <param name="names">The names collection.</param>
        /// <param name="name">The name to be checked for existence.</param>
        /// <param name="nameObject">The name object to be returned.</param>
        /// <returns>true if the name exists.</returns>
        public static bool TryGetName(this Names names, string name, out Name nameObject)
        {
            StringComparison comparison = StringComparison.OrdinalIgnoreCase;

            foreach (Name nm in names)
            {
                bool found = nm.Name.Equals(name, comparison);
                if (found)
                {
                    nameObject = nm;
                    return true;
                }
            }
            nameObject = null;
            return false;
        }

        public static void SetName(this Range range, string name)
        {
            Workbook wb = range.Worksheet.Parent;
            if (wb.Names.Contains(name)) wb.Names.Item(name).Delete();
            wb.Names.Add(name, range);
        }

        /// <summary>
        /// Returns a generic list of the  names.
        /// </summary>
        /// <param name="names">The names collection.</param>
        /// <returns>A generic list of the names.</returns>
        public static List<Name> ToList(this Names names)
        {
            List<Name> list = new List<Name>();
            foreach (Name nm in names)
                list.Add(nm);
            return list;
        }

        /// <summary>
        /// Returns an array of the names.
        /// </summary>
        /// <param name="names">The names collection.</param>
        /// <returns>An array of the names.</returns>
        public static Name[] ToArray(this Names names)
        {
            return names.ToList().ToArray();
        }
        #endregion


    }
}
