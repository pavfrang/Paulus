using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Paulus.Threads
{
    public static class SafeControlOperations
    {
        public static void SafeSetValue<ControlType, PropertyType>(this ControlType control, string controlProperty, PropertyType value) where ControlType : Control
        {
            if (control.InvokeRequired)
                control.Invoke(new Action<ControlType, string, PropertyType>(SafeSetValue<ControlType, PropertyType>),
                    control, controlProperty, value);
            else
            {
                System.Reflection.PropertyInfo p = typeof(ControlType).GetProperty(controlProperty);
                p.SetValue(control, value, null);
            }
        }

        public static void SafeSetValue<ControlType>(this ControlType control, string controlProperty, object value) where ControlType : Control
        {
            if (control.InvokeRequired)
                control.Invoke(new Action<ControlType, string, object>(SafeSetValue<ControlType>),
                    control, controlProperty, value);
            else
            {
                System.Reflection.PropertyInfo p = typeof(ControlType).GetProperty(controlProperty);
                p.SetValue(control, value, null);
            }
        }

        public static void SafeSetValue(this Control control, string controlProperty, object value)
        {
            if (control.InvokeRequired)
                control.Invoke(new Action<Control, string, object>(SafeSetValue),
                    control, controlProperty, value);
            else
            {
                System.Reflection.PropertyInfo p = typeof(Control).GetProperty(controlProperty);
                p.SetValue(control, value, null);
            }
        }

        public static void SafeSetValue(this Control parentControl, object childWhichCantInvoke, string childProperty, object value)
        {
            if (parentControl.InvokeRequired)
                parentControl.Invoke(new Action<Control, object, string, object>(SafeSetValue),
                    parentControl, childWhichCantInvoke, childProperty, value);
            else
            {
                System.Reflection.PropertyInfo p = childWhichCantInvoke.GetType().GetProperty(childProperty);
                p.SetValue(childWhichCantInvoke, value, null);
            }
        }

        //needed if there are overloads or else it cannot choose the method
        public static void SafeCallMethod(this Control control, string controlMethod, IEnumerable<Type> types, params object[] values)
        {
            if (control.InvokeRequired)
                control.Invoke(new Action<Control, string, IEnumerable<Type>, object[]>(SafeCallMethod), control, controlMethod, types, values);
            else
            {
                System.Reflection.MethodInfo p = control.GetType().GetMethod(controlMethod, types.ToArray());
                p.Invoke(control, values);
            }
        }

        //needed if there are overloads or else it cannot choose the method
        public static void SafeCallMethod(this Control parentControl, object childWhichCantInvoke, string childMethod, IEnumerable<Type> types, params object[] values)
        {
            if (parentControl.InvokeRequired)
                parentControl.Invoke(new Action<Control, object, string, IEnumerable<Type>, object[]>(SafeCallMethod),
                    parentControl, childWhichCantInvoke, childMethod, types, values);
            else
            {
                System.Reflection.MethodInfo p = childWhichCantInvoke.GetType().GetMethod(childMethod, types.ToArray());
                p.Invoke(childWhichCantInvoke, values);
            }
        }

        //used when there are overloads
        public static void SafeCallMethod(this Control control, string controlMethod, params object[] values)
        {
            if (control.InvokeRequired)
                control.Invoke(new Action<Control, string, object[]>(SafeCallMethod), control, controlMethod, values);
            else
            {
                System.Reflection.MethodInfo p = control.GetType().GetMethod(controlMethod);
                p.Invoke(control, values);
            }
        }

        public static void SafeCallMethod(this Control parentControl, object childWhichCantInvoke, string childMethod, params object[] values)
        {
            if (parentControl.InvokeRequired)
                parentControl.Invoke(new Action<Control, object, string, object[]>(SafeCallMethod),
                    parentControl, childWhichCantInvoke, childMethod, values);
            else
            {
                System.Reflection.MethodInfo p = childWhichCantInvoke.GetType().GetMethod(childMethod);
                p.Invoke(childWhichCantInvoke, values);
            }
        }

        public static PropertyType SafeGetValue<PropertyType>(this Control control,string controlProperty)
        {
            if (control.InvokeRequired)
                return (PropertyType) control.Invoke(new Func<Control, string, PropertyType>(
                    SafeGetValue<PropertyType>),control,controlProperty);
            else
            {
                System.Reflection.PropertyInfo p = typeof(Control).GetProperty(controlProperty);
                return (PropertyType)p.GetValue(control,null);
            }
        }

        public static PropertyType SafeGetValue<PropertyType>(this Control parentControl,object childWhichCantInvoke,string controlProperty)
        {
            if (parentControl.InvokeRequired)
                return (PropertyType) parentControl.Invoke(new Func<Control,object, string, PropertyType>(
                    SafeGetValue<PropertyType>),parentControl,childWhichCantInvoke,controlProperty);
            else
            {
                //get the property info
                System.Reflection.PropertyInfo p = childWhichCantInvoke.GetType().GetProperty(controlProperty);
                //return the value from the property of the child
                return (PropertyType)p.GetValue(childWhichCantInvoke, null);
            }
        }
     
        #region Common extension safe operations
        public static void SafeSetText(this TextBox txt, string text)
        {
            SafeSetValue(txt,"Text",text);
        }

        public static void SafeSetText(this ToolStripStatusLabel tst, string text)
        {
            SafeSetValue(tst.GetCurrentParent(), tst, "Text", text);
        }

        public static void SafeSetText(this ComboBox cbo, string text)
        {
            SafeSetValue(cbo, "Text", text);
        }

        public static string SafeGetText(this ComboBox cbo)
        {
            return SafeGetValue<string>(cbo, "Text");
        }
        public static string SafeGetText(this TextBox txt)
        {
            return SafeGetValue<string>(txt, "Text");
        }

        public static void SafeSetText(this ListViewItem lvwItem, string text)
        {
            SafeSetValue(lvwItem.ListView, lvwItem, "Text", text);
        }
       
        public static void SafeSetImageKey(this ListViewItem lvwItem, string imageKey)
        {
            SafeSetValue(lvwItem.ListView, lvwItem, "ImageKey", imageKey);
        }

        public static void SafeResetColors(this Button btn)
        {
            SafeSetValue(btn, "BackColor", SystemColors.Control);
            SafeSetValue(btn, "ForeColor", SystemColors.ControlText);
        }

        public static void SafeResetColors(this TextBox txt)
        {
            SafeSetValue(txt, "BackColor", SystemColors.Window);
            SafeSetValue(txt, "ForeColor", SystemColors.WindowText);
        }

        public static void SafeSetColors(this Button btn,Color backColor, Color foreColor )
        {
            SafeSetValue(btn, "BackColor", backColor);
            SafeSetValue(btn, "ForeColor", foreColor);
        }

        public static void SafeSetColors(this TextBox txt, Color backColor, Color foreColor)
        {
            SafeSetValue(txt, "BackColor", backColor);
            SafeSetValue(txt, "ForeColor", foreColor);
        }
        #endregion
    }
}
