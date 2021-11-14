using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace Paulus.Common.Converters
{
    public class EnumArrayTypeConverter<T> : TypeConverter where T : struct,IConvertible
    {
        private Type enumType;
        //ο constructor δεν πρέπει να έχει arguments
        public EnumArrayTypeConverter()
        {
            enumType = typeof(T);
        }

        public virtual bool IsReadOnly { get { return false; } }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string)) return true;
            else return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "Count: " + ((T[])value).Length.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            T[] array = (T[])value;
            List<PropertyDescriptor> pd = new List<PropertyDescriptor>();
            for (int i = 0; i < array.Length; i++)
                pd.Add(IsReadOnly ? new EnumPropertyDescriptorReadOnly<T>(array, i) : new EnumPropertyDescriptor<T>(array, i));

            return new PropertyDescriptorCollection(pd.ToArray());
        }
    }

    public class EnumArrayTypeConverterReadOnly<T> : EnumArrayTypeConverter<T> where T : struct,IConvertible
    {
        public EnumArrayTypeConverterReadOnly() : base() { }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}
