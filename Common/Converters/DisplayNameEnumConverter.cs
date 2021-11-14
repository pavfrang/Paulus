using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel; //needed for the Converter types
using System.ComponentModel.DataAnnotations; //needed for the display attribute
using System.Reflection; //needed for the FieldInfo


namespace Paulus.Common.Converters
{
    //The Enum converter is great for use at the property box.
    //To use it, put the following attribute over the enum property of an object.
    //[TypeConverter(typeof(CustomEnumConverter))]

    //For a property descriptor that is used for an array, you should override the TypeConverter property at the EnumPropertyDescriptor object:
    //public override TypeConverter Converter
    //{
    //    get
    //    {
    //        return new CustomEnumConverter(typeof(T));
    //    }
    //}
    public class DisplayNameEnumConverter : EnumConverter
    {
        public DisplayNameEnumConverter(Type type) : base(type) { this.enumType = type; }

        Type enumType;

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, value));
            DisplayAttribute display = (DisplayAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayAttribute));
            return display != null && !string.IsNullOrWhiteSpace(display.Name) ? 
                display.Name : value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, global::System.Globalization.CultureInfo culture, object value)
        {

            foreach (FieldInfo fi in enumType.GetFields())
            {
                DisplayAttribute display = (DisplayAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayAttribute));
                if (display != null && display.Name == (string)value) return Enum.Parse(enumType, fi.Name);
            }

            return Enum.Parse(enumType, (string)value);
        }

        protected override global::System.Collections.IComparer Comparer
        {
            get
            {
                return base.Comparer;
            }
        }
    }


}
