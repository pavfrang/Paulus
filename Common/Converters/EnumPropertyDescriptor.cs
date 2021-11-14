using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace Paulus.Common.Converters
{
    //This is used from the EnumArrayTypeConverter.GetProperties function. There is also a ReadOnly version below.
    //Note that the constructor must NOT contains anu arguments
    public class EnumPropertyDescriptor<T> : PropertyDescriptor where T : struct,IConvertible
    {
        protected T[] array;
        protected int index;

        public EnumPropertyDescriptor(T[] array, int index)
            : base("[" + index.ToString() + "]", null)
        {
            this.array = array;
            this.index = index;
        }

        //COMPONENT IS THE ARRAY
        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(T[]); }
        }

        public override object GetValue(object component)
        {
            return array.ElementAt(index);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(T); }
        }

        public override void ResetValue(object component)
        {
            array[index] = default(T);
        }

        public override void SetValue(object component, object value)
        {
            //component είναι το array
            array[index] = (T)value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
        public override string Description
        {
            get
            {
                Type type = typeof(T);
                FieldInfo fi = type.GetField(Enum.GetName(type, array[index]));

                DisplayAttribute display = (DisplayAttribute)Attribute.GetCustomAttribute(fi, typeof(DisplayAttribute));
                return display != null ? display.Description : null;
            }
        }
        public override TypeConverter Converter
        {
            get
            {
                return new DisplayNameEnumConverter(typeof(T));
            }
        }

    }

    public class EnumPropertyDescriptorReadOnly<T> : EnumPropertyDescriptor<T> where T : struct,IConvertible
    {
        public EnumPropertyDescriptorReadOnly(T[] array,int index) : base(array, index) { }
        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }
    }
}
