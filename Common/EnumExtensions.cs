using System;
using System.Linq;

using System.Reflection;

//using System.ComponentModel.DataAnnotations assembly is needed
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Paulus.Common.Converters;

namespace Paulus.Common
{
    public static class EnumExtensions
    {
        public static T Parse2<T>(string value, bool ignoreCase = true) where T : struct, IConvertible =>
            (T)Enum.Parse(typeof(T), value, ignoreCase);


        //example: cboLaboratories.Items.AddRange(EnumsExtensions.GetObjectValues<Laboratory>());
        public static T[] GetEnumValues<T>() where T : struct, IConvertible
        {
            Type t = typeof(T);
            if (t.IsEnum)
                return Enum.GetValues(t).Cast<T>().ToArray();
            else
                throw new ArgumentException("T must be an enumerated type.");
        }

        /// <summary>
        /// Returns true only if the value is defined within the enumeration except the values in the exception list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public static bool IsWellDefined<T>(this T value, params T[] exceptions) where T : struct, IConvertible =>
            Enum.GetValues(typeof(T)).Cast<T>().
                Except(exceptions).Contains(value);

        public static bool IsWellDefined<T>(this T value) where T : struct, IConvertible =>
            Enum.GetValues(typeof(T)).Cast<T>().Contains(value);


        public static bool EnumValueExists<T>(this T value) where T : struct, IConvertible
        {
            return Enum.IsDefined(typeof(T), value);
            //T[] enumValues = GetEnumValues<T>();
            //return enumValues.Contains(value);
        }

        //example: cboLaboratories.Items.AddRange(EnumsExtensions.GetObjectValues<Laboratory>());
        //works for VS2010, .NET 4.0
        public static object[] GetEnumValuesAsObjects<T>() where T : struct, IConvertible
        {
            Type t = typeof(T);
            if (t.IsEnum)
                return Enum.GetValues(t).Cast<object>().ToArray();
            else
                throw new ArgumentException("T must be an enumerated type.");
        }

        /// <summary>
        /// Retrieves the Name property of the Display attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayName<T>(this T enumValue) where T : struct, IConvertible
        {
            ////Type enumType = typeof(T);
            ////FieldInfo fi = enumType.GetField(Enum.GetName(enumType, enumValue));
            ////DisplayNameAttribute displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayNameAttribute));
            ////return displayName != null ? displayName.Text : enumValue.ToString();

            //return enumValue.GetAttribute(typeof(DisplayAttribute), "Name") ?? enumValue.ToString();


            //returns the value name or the DisplayName property if it exists
            DisplayNameEnumConverter c = new DisplayNameEnumConverter(typeof(T));
            //return c.ConvertTo(enumValue,typeof(string)).ToString()
            return c.ConvertToString(enumValue);
        }

        //public static string GetDisplayName<T>(this T enumValue) where T : struct, IConvertible
        //{
        //    Type enumType = typeof(T);
        //    FieldInfo fi = enumType.GetField(Enum.GetName(enumType, enumValue));

        //    DisplayAttribute display = (DisplayAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayAttribute));
        //    return display != null ? display.Description : enumValue.ToString();
        //}

        /// <summary>
        /// Retrieves the Description attribute value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayDescription<T>(this T enumValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, enumValue));

            DisplayAttribute display = (DisplayAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayAttribute));
            return display != null ? display.Description : enumValue.ToString();
        }

        public static string GetDescription<T>(this T enumValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, enumValue));
            DescriptionAttribute description = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            return description != null ? description.Description : enumValue.ToString();
            //return GetAttribute<T, DescriptionAttribute>(enumValue, "Description");
        }

        public static string GetAttribute<TEnum, TAttribute>(this TEnum enumValue,
            string attributePropertyName) where TEnum : struct, IConvertible
        {
            Type enumType = typeof(TEnum);
            Type attributeType = typeof(TAttribute);
            return GetAttribute(enumValue, attributeType, attributePropertyName);
        }

        private static string GetAttribute<T>(this T enumValue, Type attributeType,
            string attributePropertyName) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, enumValue));

            Attribute attribute = Attribute.GetCustomAttribute(fi, attributeType);
            PropertyInfo pi = attributeType.GetProperty(attributePropertyName);
            //implies returns attribute.<atributePropertyName>
            return attribute != null ? pi.GetValue(attribute, null).ToString() :
                enumValue.ToString();
        }

    }

}
