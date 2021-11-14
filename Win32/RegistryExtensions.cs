using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Paulus.System
{
    public static class RegistryExtensions
    {
        //all custom keys are under CU\Software\PaulusDeviceManager
        /// <summary>
        /// Loads a dictionary of name/value pairs from the registry under the key CU\SOFTWARE\PaulusDeviceManager\[subKeyName].
        /// </summary>
        /// <param name="subKeyName">The name of the subcategory that contains the keys. It may be combined with other subkeys eg. it may contain the value "Device\Startup".</param>
        /// <exception cref="System.ArgumentException">Thrown when subKeyName is an empty string or null.</exception>
        /// <returns>The retrieved name/value pairs or null if no pairs are retrieved.</returns>
        public static Dictionary<string, string> LoadValuesFromRegistry(string subKeyName, bool usePaulusKey=true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subKeyName)) throw new ArgumentException("The key may not be empty or null.", "subKeyName");
                RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    "Software\\" + (usePaulusKey? "PaulusDeviceManager\\":"") + subKeyName);
                if (key != null)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    string[] valueNames = key.GetValueNames();
                    foreach (string valueName in valueNames)
                        dic.Add(valueName, key.GetValue(valueName).ToString());
                    return dic;
                }
                else return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves a dictionary of name/value pairs to the registry under the key CU\SOFTWARE\PaulusDeviceManager\[subKeyName].
        /// </summary>
        /// <param name="subKeyName">The name of the subcategory that contains the keys. It may be combined with other subkeys eg. it may contain the value "Device\Startup".</param>
        /// <param name="nameValuePairs">The dictionary that contains the name/value pairs.</param>
        /// <exception cref="System.ArgumentException">Thrown when subKeyName is an empty string or null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the nameValuePairs is null.</exception>"
        /// <returns>true if the operation is successful and false if the operation is unsuccessful.</returns>
        public static bool SaveValuesToRegistry(string subKeyName, Dictionary<string, string> nameValuePairs, bool usePaulusKey=true)
        {
            if (string.IsNullOrWhiteSpace(subKeyName)) throw new ArgumentException("The key may not be empty or null.", "subKeyName");
            if (nameValuePairs == null) throw new ArgumentNullException("The dictionary may not be null.", "nameValuePairs");

            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    "Software\\" + (usePaulusKey ? "PaulusDeviceManager\\" : "") + subKeyName);
                if (key != null)
                {
                    foreach (var entry in nameValuePairs)
                        key.SetValue(entry.Key, entry.Value);

                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        //typically used at the Load event, or at the constructor after the InitializeComponent()
        public static bool LoadStartupPositionFromRegistry(this Form frm, string subKeyName)
        {
            try
            {
                var values = LoadValuesFromRegistry(subKeyName);

                if (values != null)
                {
                    frm.Width = int.Parse(values["width"]);
                    frm.Height = int.Parse(values["height"]);
                    frm.Left = int.Parse(values["left"]);
                    frm.Top = int.Parse(values["top"]);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        //typically at the closing event
        public static bool SaveLastPositionToRegistry(this Form frm, string subKeyName)
        {
            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                values.Add("width", frm.Width.ToString());
                values.Add("height", frm.Height.ToString());
                values.Add("left", frm.Left.ToString());
                values.Add("top", frm.Top.ToString());
                return SaveValuesToRegistry(subKeyName, values);
            }
            catch { return false; }

        }

    }
}
