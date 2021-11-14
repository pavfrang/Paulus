using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Configuration;

namespace Paulus.Forms
{
    public class ExtendedFormSettings : ApplicationSettingsBase
    {
        private static ExtendedFormSettings defaultInstance =
            ((ExtendedFormSettings)(ApplicationSettingsBase.Synchronized(new ExtendedFormSettings())));

        public static ExtendedFormSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting]
        [DefaultSettingValueAttribute("0, 0")]
        public Point FormLocation
        {
            get { return (Point)(this["FormLocation"]); }
            set { this["FormLocation"] = value; }
        }

        //[ApplicationScopedSetting()]
        [UserScopedSetting]
        [DefaultSettingValueAttribute("0, 0")]
        public Size FormSize
        {
            get { return (Size)this["FormSize"]; }
            set { this["FormSize"] = value; }
        }

    }


}
