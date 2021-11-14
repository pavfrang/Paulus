using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Excel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExcelRowAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ExcelColumnAttribute : Attribute
    {
        // This is a positional argument
        public ExcelColumnAttribute(string columnHeader)
        {
            this._columnHeader = columnHeader;
        }

        public ExcelColumnAttribute() { }

        readonly string _columnHeader;
        public string ColumnHeader
        {
            get { return _columnHeader; }
        }
    }

}
