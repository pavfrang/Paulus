using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Paulus.Common
{
    public static class ReflectionExtensions
    {
        public static bool HasAttribute(this MemberInfo member, Type attributeType)
        {
            return member.GetCustomAttributes(attributeType, true).Length > 0;
        }

        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), true).Length > 0;
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            object[] attrs = member.GetCustomAttributes(typeof(T), true);
            return attrs.Length > 0 ? (T)attrs[0] : null;
        }

        //this returns multiple occurences of the same attribute
        public static List<T> GetAttributes<T>(this MemberInfo member) where T : Attribute
        {
            object[] attrs = member.GetCustomAttributes(typeof(T), true);
            return attrs.Length > 0 ? attrs.Cast<T>().ToList() : null;
        }

        public static bool IsPublic(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property: //assumed public if the get method is public
                    //((PropertyInfo)member).CanRead && ((PropertyInfo)member).GetSetMethod(true) != null;
                    MethodInfo getMethod = ((PropertyInfo)member).GetGetMethod();
                    return getMethod != null && getMethod.IsPublic;
                case MemberTypes.Field:
                    return ((FieldInfo)member).IsPublic;
                case MemberTypes.Method:
                    return ((MethodInfo)member).IsPublic;
                case MemberTypes.Event:
                    return ((EventInfo)member).GetAddMethod(true).IsPublic;
                default:
                    return false;
            }

        }

        public static bool IsPropertyFieldOrMethod(this MemberInfo member)
        {
            return member.MemberType == MemberTypes.Method || member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field;
        }

        public static bool IsPropertyOrField(this MemberInfo member)
        {
            return member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field;
        }

        /// <summary>
        /// Returns the description attribute for a class declaration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <example>
        /// [Description( "Environics Series 2000")]
        /// public class GasMixerCommander : DeviceCommander
        /// </example>
        public static string GetDescription<T>()
        {
            DescriptionAttribute description = typeof(T).GetAttribute<DescriptionAttribute>();
            return description?.Description ?? typeof(T).Name;
        }
        public static string GetDescription(this Type t)
        {
            DescriptionAttribute description = t.GetAttribute<DescriptionAttribute>();
            return description?.Description ?? t.Name;
        }
    }
}
